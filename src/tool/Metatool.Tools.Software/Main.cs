using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Metatool.Service;
using Microsoft.Extensions.Logging;
using static Metatool.Service.Key;

namespace Metatool.Tools.Software
{
    public partial class SoftwareTool : ToolBase
    {
        private readonly IShell _shell;
        private readonly IVirtualDesktopManager _virtualDesktopManager;
        private readonly IWindowManager _windowManager;
        private readonly IContextVariable _contextVariable;

        public SoftwareTool(ICommandManager commandManager, IKeyboard keyboard, IConfig<Config> config, IShell shell,
            IVirtualDesktopManager virtualDesktopManager, IWindowManager windowManager,
            IContextVariable contextVariable)
        {
            _shell = shell;
            _virtualDesktopManager = virtualDesktopManager;
            _windowManager = windowManager;
            _contextVariable = contextVariable;

            var folders = config.CurrentValue.ConfigFolders;
            foreach (var folder in folders)
            {
                loadConfigInFolder(folder);
            }

            RegisterCommands();
        }

        private void loadConfigInFolder(string folder)
        {
            var toolDir = Context.ToolDir<SoftwareTool>();
            folder = Context.ParsePath(folder, toolDir, typeof(SoftwareTool));
            var files = GetFiles(folder);
            ConfigShortcuts(files, folder);
        }

        void ConfigShortcuts(IEnumerable<string> files, string rootFolder) {
                        var hotKeys = new List<IHotkeyTrigger>();

            foreach (var file in files)
            {
                var keys = file.Replace(rootFolder, "").Split(Path.DirectorySeparatorChar).Select(k => k.Trim())
                    .Where(k => !string.IsNullOrEmpty(k)).ToArray();
                var fileName = keys[^1];

                fileName = Path.GetFileNameWithoutExtension(fileName);
                var hotkeyTrigger = HotkeyTrigger.Parse(fileName);

                keys[^1] = hotkeyTrigger.Hotkey;
                var sb = new StringBuilder();
                foreach (var key in keys)
                {
                    sb.Append(key);
                    if (!key.EndsWith('+')) sb.Append(',');
                }

                hotkeyTrigger.Hotkey = sb.ToString();
                hotKeys.Add(hotkeyTrigger);
                if (Path.GetExtension(file) == ".lnk")
                {
                    ConfigShortcut(file, hotkeyTrigger);
                }
            }
        }
        bool ConfigShortcut(string file, IHotkeyTrigger hotkeyTrigger)
        {
            var shortcutConfig = _shell.ReadShortcut(file);
            if (shortcutConfig == null) return false;

            var conf = new SoftwareActionConfig();
            if (!string.IsNullOrEmpty(shortcutConfig.Comment.Trim()))
            {
                try
                {
                    conf = SoftwareActionConfig.Parse(shortcutConfig.Comment);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, $"Could not parse Commit in file:{file} - {shortcutConfig.Comment} ");
                    return false;
                }
            }

            conf.Handled &= hotkeyTrigger.Handled;

            hotkeyTrigger.OnEvent(async e =>
            {
                try
                {
                    await LaunchShortcut(e, file, conf, shortcutConfig);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Can not run shortcut: {file}");
                }
            });
            return true;
        }

        /// <summary>
        /// "abc${variable}"
        /// </summary>
        /// <param name="varString"></param>
        /// <returns></returns>
        static async Task<string> ExpandVariableString(IContextVariable contextVariable, string varString,
            Func<object, string> variableConverter)
        {
            if (string.IsNullOrEmpty(varString))
                return "";

            var sb = new StringBuilder();
            var inVariable = false;
            var varStartIndex = -1;
            for (var i = 0; i < varString.Length; i++)
            {
                if (i < varString.Length - 3 /*${?}*/ && varString[i] == '$' && varString[i + 1] == '{')
                {
                    inVariable = true;
                    varStartIndex = i + 2;
                }

                if (!inVariable) sb.Append(varString[i]);

                if (inVariable && varString[i] == '}')
                {
                    var key = varString.Substring(varStartIndex, i - varStartIndex);

                    var variable = await contextVariable.GetVariable<object>(key);

                    var var = variableConverter(variable);
                    sb.Append(var);
                    inVariable = false;
                    varStartIndex = -1;
                }
            }

            return sb.ToString();
        }

        public async Task LaunchShortcut(IKeyEventArgs e, string shortcut, SoftwareActionConfig config,
            ShortcutLink shortcutLink)
        {
            e.Handled = config.Handled;
            var shell = Services.Get<IShell>();

            if (config.ShowIfOpened && !string.IsNullOrEmpty(shortcutLink.TargetPath) /*winstore app*/)
            {
                var exePath = Path.GetFullPath(shortcutLink.TargetPath);
                var exeName = Path.GetFileNameWithoutExtension(shortcutLink.TargetPath);
                var processes = await
                    _virtualDesktopManager.GetProcessesOnCurrentVirtualDesktop(exeName,
                        p =>
                        {
                            if (p.MainModule == null) return false;

                            var processPath = Path.GetFullPath(p.MainModule.FileName);
                            if (string.Compare(exePath, processPath, StringComparison.InvariantCultureIgnoreCase) != 0)
                                return false;

                            if (string.IsNullOrEmpty(config.ShowIfOpenedTitle))
                                return true;

                            var regex = new Regex(config.ShowIfOpenedTitle);
                            return regex.IsMatch(p.MainWindowTitle);
                        });

                var process = processes.FirstOrDefault();

                var hWnd = process?.MainWindowHandle;

                if (hWnd != null)
                {
                    _windowManager.Show(hWnd.Value);
                    return;
                }
            }

            //if (string.IsNullOrEmpty(config.Args))
            //{
            //    shell.RunWithExplorer(shortcut);
            //}

            _contextVariable.NewGeneration();
            var arg = await ExpandVariableString(_contextVariable, config.Args, o =>
            {
                switch (o)
                {
                    case string str:
                        return str;
                    case string[] strA:
                        return string.Join(" ", strA);
                    case null:
                        return "";
                    default:
                        throw new Exception("unsupported context variable type when parse shortcut arguments");
                }
            });

            switch (config.RunMode)
            {
                case RunMode.Inherit:
                    //if(string.IsNullOrEmpty(arg))
                    //    shell.RunWithExplorer(shortcut);
                    //else 
                    shell.RunWithPowershell(shell.NormalizeCmd(shortcut), arg);

                    break;
                case RunMode.Admin:
                    shell.RunWithPowershell(shell.NormalizeCmd(shortcut), arg, true);
                    //shell.RunWithCmd(shell.NormalizeCmd(shortcut) + $" {arg}", asAdmin: true);
                    break;
                case RunMode.User:
                    shell.RunAsNormalUser(shortcut, arg);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override bool OnLoaded()
        {
            Logger.LogInformation($"all other tools are already created here");
            return base.OnLoaded();
        }


        IEnumerable<string> GetFiles(string path)
        {
            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                var dirName = Path.GetFileName(dir);
                if (dirName.StartsWith("_")) continue;

                var fs = GetFiles(dir);
                foreach (var file in fs)
                {
                    yield return file;
                }
            }

            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                if (file.StartsWith("_")) continue;

                yield return Path.Combine(path, file);
            }
        }
    }
}