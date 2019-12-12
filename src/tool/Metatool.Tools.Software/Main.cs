using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Metatool.Service;
using Metatool.Tools.LibToolDemo;
using Microsoft.Extensions.Logging;
using static Metatool.Service.Key;

namespace Metatool.Tools.Software
{
    public class SoftwareTool : ToolBase
    {
        public ICommandToken<IKeyEventArgs> CommandA;
        public IKeyCommand                  CommandB;


        public SoftwareTool(ICommandManager commandManager, IKeyboard keyboard, IConfig<Config> config)
        {
            var folder = config.CurrentValue.SoftwareFolder;
            var files  = GetFiles(folder);

            var hotKeys = new List<IHotkeyTrigger>();

            foreach (var file in files)
            {
                var keys = file.Replace(folder, "").Split(Path.DirectorySeparatorChar).Select(k => k.Trim())
                    .Where(k => !string.IsNullOrEmpty(k)).ToArray();
                var fileName = keys[keys.Length - 1];

                var extIndex = fileName.LastIndexOf('.');
                if (extIndex != fileName.Length - 1 && extIndex > fileName.Length - 1 - 8)
                {
                    fileName = fileName.Substring(0, extIndex);
                }

                var hotKeyTrigger = HotkeyTrigger.Parse(fileName);

                keys[keys.Length - 1] = hotKeyTrigger.Hotkey;

                var sb = new StringBuilder();
                foreach (var key in keys)
                {
                    sb.Append(key);
                    if (!key.EndsWith('+')) sb.Append(',');
                }

                hotKeyTrigger.Hotkey = sb.ToString();
                hotKeys.Add(hotKeyTrigger);
                var conf = SoftwareActionConfig.Parse(hotKeyTrigger.Context);
                conf.Handled |= hotKeyTrigger.Handled;
                hotKeyTrigger.OnEvent( async e =>  await LaunchShortcut(e, file,conf));
            }


            CommandA = commandManager.Add(keyboard.OnDown(Caps + A),
                e => { Logger.LogInformation($"{nameof(SoftwareTool)}: Caps+A triggered!!!!!!!"); });
            CommandB = (Caps + B).OnDown(e => Logger.LogWarning("Caps+B pressed!!!"));

            Logger.LogInformation(config.CurrentValue.Option2.ToString());
            RegisterCommands();
        }

        [Flags]
        public enum ExplorerPathPara
        {
            Selected             = 1,
            CurrentDir           = 2,
            SelectedOrCurrentDir = 3
        }

        public class SoftwareActionConfig
        {
            public bool Handled { get; set; } = true;
            public string ActionId { get; set; } = "ShortcutLaunch";
            public string Param1 { get; set; }

            public static SoftwareActionConfig Parse(IDictionary<string, string> prop)
            {
                var config = new SoftwareActionConfig();

                if (prop == null) return config;

                try
                {
                    var d                                                 = nameof(SoftwareActionConfig.ActionId);
                    if (prop.TryGetValue(d, out var des)) config.ActionId = des;
                    d = nameof(SoftwareActionConfig.Handled);
                    if (prop.TryGetValue(d, out des)) config.Handled = bool.Parse(des);
                    d = nameof(SoftwareActionConfig.Param1);
                    if (prop.TryGetValue(d, out des)) config.Param1 = des;
                }
                catch (Exception e)
                {
                    Services.CommonLogger.LogError(e,
                        $"SoftwareActionConfig Parse: cannot parse SoftwareActionConfig properties + {e.Message}");
                }

                return config;
            }
        }

        public async Task LaunchShortcut(IKeyEventArgs e, string shortcut, SoftwareActionConfig config)
        {
            e.Handled = config.Handled;
            var shell = Services.Get<IShell>();
            if (string.IsNullOrEmpty(config.Param1))
            {
                shell.RunWithExplorer(shortcut);
                return;
            }

            // todo: if ActionId != "ShortcutLaunch
            if (Enum.TryParse(config.Param1, out ExplorerPathPara explorerPath))
            {
                var windowManager = Services.Get<IWindowManager>();
                if (!windowManager.CurrentWindow.IsExplorerOrOpenSaveDialog)
                {
                    shell.RunWithExplorer(shortcut);
                    return;
                }

                var fileExplorer = Services.Get<IFileExplorer>();
                if ((explorerPath & ExplorerPathPara.Selected) == ExplorerPathPara.Selected)
                {
                    var paths = await fileExplorer.GetSelectedPaths(windowManager.CurrentWindow.Handle);
                    if (paths.Length != 0)
                    {
                        foreach (var path in paths)
                        {
                            shell.RunWithCmd(shell.NormalizeCmd(shortcut, path));
                        }

                        return;
                    }
                }

                if ((explorerPath & ExplorerPathPara.CurrentDir) == ExplorerPathPara.CurrentDir)
                {
                    var path = await fileExplorer.Path(windowManager.CurrentWindow.Handle);
                    shell.RunWithCmd(shell.NormalizeCmd(shortcut, path));
                    return;
                }
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
                if (dir.StartsWith("_")) continue;

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