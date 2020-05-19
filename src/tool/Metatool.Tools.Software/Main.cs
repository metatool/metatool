using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Metatool.Service;
using Microsoft.Extensions.Logging;
using static Metatool.Service.Key;

namespace Metatool.Tools.Software
{
    public class SoftwareTool : ToolBase
    {
        private readonly IVirtualDesktopManager _virtualDesktopManager;
        private readonly IWindowManager _windowManager;
        private readonly IContextVariable _contextVariable;
        public ICommandToken<IKeyEventArgs> CommandA;
        public IKeyCommand CommandB;

        public SoftwareTool(ICommandManager commandManager, IKeyboard keyboard, IConfig<Config> config, IShell shell,
            IVirtualDesktopManager virtualDesktopManager, IWindowManager windowManager,
            IContextVariable contextVariable)
        {
            _virtualDesktopManager = virtualDesktopManager;
            _windowManager = windowManager;
            _contextVariable = contextVariable;
            var folders = config.CurrentValue.ConfigFolders;
            foreach (var folder in folders)
            {
                loadConfigInFolder(folder, shell);
            }

            CommandA = commandManager.Add(keyboard.OnDown(Caps + A),
                e => { Logger.LogInformation($"{nameof(SoftwareTool)}: Caps+A triggered!!!!!!!"); });
            CommandB = (Caps + B).OnDown(e => Logger.LogWarning("Caps+B pressed!!!"));

            Logger.LogInformation(config.CurrentValue.Option2.ToString());
            RegisterCommands();
        }

        private void loadConfigInFolder(string folder, IShell shell)
        {
            var toolDir = Context.ToolDir<SoftwareTool>();

            folder = Context.ParsePath(folder, toolDir, typeof(SoftwareTool));
            var files = GetFiles(folder);

            var hotKeys = new List<IHotkeyTrigger>();

            foreach (var file in files)
            {
                var keys = file.Replace(folder, "").Split(Path.DirectorySeparatorChar).Select(k => k.Trim())
                    .Where(k => !string.IsNullOrEmpty(k)).ToArray();
                var fileName = keys[^1];

                fileName = Path.GetFileNameWithoutExtension(fileName);
                var hotKeyTrigger = HotkeyTrigger.Parse(fileName);

                keys[^1] = hotKeyTrigger.Hotkey;
                var sb = new StringBuilder();
                foreach (var key in keys)
                {
                    sb.Append(key);
                    if (!key.EndsWith('+')) sb.Append(',');
                }

                hotKeyTrigger.Hotkey = sb.ToString();
                hotKeys.Add(hotKeyTrigger);
                if (Path.GetExtension(file) == ".lnk")
                {
                    var shortcutConfig = shell.ReadShortcut(file);
                    if (shortcutConfig == null) continue;

                    var conf = new SoftwareActionConfig();
                    if (!string.IsNullOrEmpty(shortcutConfig.Comment))
                    {
                        try
                        {
                            conf = SoftwareActionConfig.Parse(shortcutConfig.Comment);
                        }
                        catch (Exception)
                        {
                            Logger.LogError($"Could not parse Commit in file:{file} - {shortcutConfig.Comment} ");
                        }
                    }

                    conf.Handled |= hotKeyTrigger.Handled;


                    hotKeyTrigger.OnEvent(async e =>
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
                }
            }
        }
        public enum RunMode
        {
            Inherit,
            Admin,
            User
        }

        public class SoftwareActionConfig
        {
            public bool Handled { get; set; } = true;
            public string ActionId { get; set; } = "ShortcutLaunch";
            public string Args { get; set; }
            public bool ShowIfOpened { get; set; } = true;

            /// <summary>
            /// regex
            /// </summary>
            public string ShowIfOpenedTitle { get; set; }

            public RunMode RunMode { get; set; } = RunMode.Inherit;

            public static SoftwareActionConfig Parse(string jsonString)
            {
                if (!jsonString.TrimStart().StartsWith('{') || string.IsNullOrEmpty(jsonString))
                    return new SoftwareActionConfig();
                var options = new JsonSerializerOptions();
                options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                try
                {
                    return JsonSerializer.Deserialize<SoftwareActionConfig>(jsonString, options);
                }
                catch (Exception e)
                {
                    Services.CommonLogger.LogError(e,
                        $"SoftwareActionConfig Parse: cannot parse SoftwareActionConfig properties + {e.Message}");
                    throw;
                }
            }
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
            int varStartIndex = -1;
            for (int i = 0; i < varString.Length; i++)
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

        public override void OnUnloading()
        {
            CommandA.Remove();
            base.OnUnloading();
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