using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IWshRuntimeLibrary;
using Metatool.Service;
using Microsoft.Extensions.Logging;
using File = System.IO.File;

namespace Metatool.Utils;

public class Shell : IShell
{
	private readonly ILogger _logger = Services.Get<ILogger<Shell>>();
	static string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    public Shell(IContextVariable contextVariable)
	{
		contextVariable.AddVariable("IShell.SelectedPaths", async () => await SelectedPaths());
        contextVariable.AddVariable("IShell.CurrentDirectoryOrHome", async () => await CurrentDirectory() ?? homePath);
        contextVariable.AddVariable("IShell.CurrentDirectory", async () => await CurrentDirectory());
		contextVariable.AddVariable("IShell.SelectedPathsOrCurrentDirectory", async () =>
		{
			var paths = await SelectedPaths();

			return (paths != null && paths.Length != 0)
				? paths
				: [await CurrentDirectory()];
		});
	}

	public async Task<string[]> SelectedPaths()
	{
		var fileExplorer = Services.Get<IFileExplorer>();
		var windowManager = Services.Get<IWindowManager>();
		if (!windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog)
		{
			return null;
		}

		var r = await fileExplorer.GetSelectedPaths(windowManager.CurrentWindow.Handle);
		return r;
	}

	public async Task<string> CurrentDirectory()
	{
		var fileExplorer = Services.Get<IFileExplorer>();
		var windowManager = Services.Get<IWindowManager>();
		if (!windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog)
		{
			return null;
		}

		return await fileExplorer.CurrentDirectory(windowManager.CurrentWindow.Handle);
	}

	/// <summary>
	/// in its own process without window
	/// </summary>
	public CommandResult Run(string commandPath, string arguments, string workingDirectory = null)
	{
		var startInformation = new ProcessStartInfo($"{commandPath}")
		{
			CreateNoWindow = true,
			Arguments = arguments ?? "",
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			WorkingDirectory = workingDirectory ?? System.Environment.CurrentDirectory
		};
		;
		var process = new Process {StartInfo = startInformation};
		process.OutputDataReceived += (s, e) =>
		{
			if (!string.IsNullOrWhiteSpace(e.Data))
			{
				_logger.LogDebug(e.Data);
			}
		};
		process.ErrorDataReceived += (s, e) =>
		{
			if (!string.IsNullOrWhiteSpace(e.Data))
			{
				_logger.LogDebug(e.Data);
			}
		};
		process.Start();
		var standardOut = process.StandardOutput.ReadToEnd();
		if (!string.IsNullOrEmpty(standardOut)) _logger.LogInformation(standardOut);
		var standardError = process.StandardError.ReadToEnd();
		if (!string.IsNullOrEmpty(standardError)) _logger.LogWarning(standardError);

		process.WaitForExit();
		return new CommandResult(process.ExitCode, standardOut, standardError);
	}

	// i.e. if run as admin Chrome could not load extensions
	public void RunAsNormalUser(string cmd, params string[] args)
	{
		if (Context.IsElevated)
		{
			var c = args.ToList();
			c.Insert(0, cmd);
			var exeWithArgs = NormalizeCmd(c.ToArray());
			var s = $"start \"\" {exeWithArgs} \n exit 0";
			var tempBat = Path.Combine(Path.GetTempPath(), "t.bat");
			File.WriteAllText(tempBat, s);
			RunWithExplorer(tempBat);
		}
		else
		{
			new Process
			{
				StartInfo =
				{
					UseShellExecute = true,
					FileName = cmd,
					Arguments = NormalizeCmd(args)
				}
			}.Start();
		}
	}

	private readonly char[] _specialChars = new[]
		{' ', '&', '<', '>', '[', ']', '{', '}', '^', '=', ';', '!', '\'', '+', ',', '`', '~'};

	public string NormalizeCmd(params string[] cmdArgsSerials)
	{
		return string.Join(" ", cmdArgsSerials.Select(arg => arg.Any(_specialChars.Contains) ? $"\"{arg}\"" : arg));
	}

	// Process.Start would keep the process parent/child structure, if the parent exit, the child would exit.
	// so we use cmd to make a workaround
	// https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/cmd
	// https: //ss64.com/nt/cmd.html
	// could run *.lnk with args
	public void RunWithCmd(string cmdWithArgs, bool asAdmin = false, string workingDir = null)
	{
		var proc = new Process
		{
			StartInfo = new System.Diagnostics.ProcessStartInfo()
			{
				FileName = "cmd.exe",
				Arguments = "/C \"" + cmdWithArgs + "\"",
				UseShellExecute = false,
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				WorkingDirectory = workingDir ?? Context.AppDirectory,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
			}
		};
		if (asAdmin) proc.StartInfo.Verb = "runas";
		proc.Start();
	}

	public void RunWithPowershell(string filePath, string args, bool asAdmin = false, string workingDir = null)
	{
		var cmd =
			$"Start-Process -FilePath '{filePath}' {(asAdmin ? "-Verb RunAs" : "")} {(string.IsNullOrEmpty(args) ? "" : $"-ArgumentList \"{args}\"")}";
		var encodedCmd = Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(cmd));
		var proc = new Process
		{
			StartInfo = new System.Diagnostics.ProcessStartInfo()
			{
				FileName = "powershell.exe",
				Arguments = $"-encodedCommand {encodedCmd} {(string.IsNullOrEmpty(workingDir) ? "" : $"-WorkingDirectory \"{workingDir}\"")}",
				UseShellExecute = false,
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				WorkingDirectory = workingDir ?? Context.AppDirectory,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError =  true
			},
			EnableRaisingEvents = true
		};
		proc.ErrorDataReceived += (s, d) =>
		{
			if(d.Data!=null)
				_logger.LogInformation(d.Data);
		};
		proc.OutputDataReceived += (s, d) => _logger.LogInformation(d.Data);

		if (asAdmin) proc.StartInfo.Verb = "runas";
		proc.Start();
		proc.BeginErrorReadLine();
		proc.BeginOutputReadLine();
	}
	/// <summary>
	/// Process.Start would keep the process parent/child structure, if the parent exit, the child would exit.
	/// so we use explorer to make a workaround
	///
	/// this could run *.lnk and *.bat
	/// </summary>
	public void RunWithExplorer(string filePath, string workingDir = null)
	{
		var proc = new Process
		{
			StartInfo = new ProcessStartInfo()
			{
				FileName = "explorer.exe",
				ArgumentList = {filePath},
				UseShellExecute = false,
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				WorkingDirectory = workingDir ?? Context.AppDirectory
			}
		};

		proc.Start();
	}

	public void CreateShortcut(string targetPath, string shortcutPath, string hotkey = "",
		string description = "", bool isAdmin = false)
	{
		var exists = File.Exists(shortcutPath);
		if (exists) return;

		//var shDesktop = (object) "Desktop";
		var shell = new WshShell();
		var shortcut = (IWshShortcut) shell.CreateShortcut(shortcutPath);
		shortcut.Description = description;
		shortcut.Hotkey = hotkey;
		shortcut.TargetPath = targetPath;
		shortcut.Save();
		if (isAdmin)
		{
			// admin hack
			using var fs = new FileStream(shortcutPath, FileMode.Open, FileAccess.ReadWrite);
			fs.Seek(21, SeekOrigin.Begin);
			fs.WriteByte(0x22);
		}
	}

	public ShortcutLink ReadShortcut(string shortcutPath)
	{
		var exists = File.Exists(shortcutPath);
		if (!exists) return null;

		IWshShell shell = new WshShell();
		try
		{
			var lnk = shell.CreateShortcut(shortcutPath) as IWshShortcut;
			return new ShortcutLink(lnk.TargetPath, lnk.Arguments, lnk.Description, lnk.FullName, lnk.IconLocation,
				lnk.Hotkey, lnk.WindowStyle, lnk.WorkingDirectory);
		}
		catch
		{
			_logger.LogWarning($"Can not read shortcut info: {shortcutPath}, make sure it exist.");
			return null;
		}
	}
}