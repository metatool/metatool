using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Metatool.Service;
using Microsoft.Extensions.Logging;
using static Metatool.Service.Key;

namespace Metatool.Tools.Software
{
    public class SoftwareTool : ToolBase
    {
        private readonly IContextVariable _contextVariable;
        public ICommandToken<IKeyEventArgs> CommandA;
        public IKeyCommand CommandB;


        public SoftwareTool(ICommandManager commandManager, IKeyboard keyboard, IConfig<Config> config, IShell shell,
            IContextVariable contextVariable)
        {
            _contextVariable = contextVariable;
            var folder = config.CurrentValue.SoftwareFolder;
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

                    hotKeyTrigger.OnEvent(async e => await LaunchShortcut(e, file, conf));
                }
            }


            CommandA = commandManager.Add(keyboard.OnDown(Caps + A),
                e => { Logger.LogInformation($"{nameof(SoftwareTool)}: Caps+A triggered!!!!!!!"); });
            CommandB = (Caps + B).OnDown(e => Logger.LogWarning("Caps+B pressed!!!"));

            Logger.LogInformation(config.CurrentValue.Option2.ToString());
            RegisterCommands();
        }

        public class SoftwareActionConfig
        {
            public bool Handled { get; set; } = true;
            public string ActionId { get; set; } = "ShortcutLaunch";
            public string Args { get; set; }

            public static SoftwareActionConfig Parse(string jsonString)
            {
                if (!jsonString.TrimStart().StartsWith('{') || string.IsNullOrEmpty(jsonString))
                    return new SoftwareActionConfig();

                try
                {
                    return JsonSerializer.Deserialize<SoftwareActionConfig>(jsonString);
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

        public async Task LaunchShortcut(IKeyEventArgs e, string shortcut, SoftwareActionConfig config)
        {
            e.Handled = config.Handled;
            var shell = Services.Get<IShell>();

            if (string.IsNullOrEmpty(config.Args))
            {
                shell.RunWithExplorer(shortcut);
                return;
            }

            _contextVariable.NewGeneration();
            var arg = await ExpandVariableString(_contextVariable, config.Args, o =>
            {
                switch (o)
                {
                    case string str:
                        return str;
                    case string[] strA:
                        return string.Join(" ", strA);
                    default:
                        throw new Exception("unsupported context variable type when parse shortcut arguments");
                }
            });

            shell.RunWithCmd(shell.NormalizeCmd(shortcut) + $" {arg}");
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