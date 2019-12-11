using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using Metatool.Service;
using Metatool.Tools.LibToolDemo;
using Microsoft.Extensions.Logging;
using static Metatool.Service.Key;

namespace Metatool.Tools.Software
{
    public class SoftwareTool : ToolBase
    {
        public ICommandToken<IKeyEventArgs> CommandA;
        public IKeyCommand CommandB;

        public SoftwareTool(ICommandManager commandManager, IKeyboard keyboard, IConfig<Config> config)
        {
            var folder = config.CurrentValue.SoftwareFolder;
            var files = GetFiles(folder);

            var hotKeys = new List<IHotkey>();

            foreach (var file in files)
            {
                var keys = file.Replace(folder, "").Split(Path.DirectorySeparatorChar).Select(k=>k.Trim()).Where(k=> !string.IsNullOrEmpty(k)).ToArray();
                var fileName = keys[keys.Length - 1];
                var triggerIndex = fileName.IndexOf('&');
                if (triggerIndex != -1) keys[keys.Length - 1] = fileName[triggerIndex + 1].ToString().ToUpper();
                else
                {
                    keys[keys.Length - 1] = fileName[0].ToString().ToUpper();
                }

                var sb = new StringBuilder();
                foreach ( var key in keys)
                {
                    sb.Append(key);
                    if (!key.EndsWith('+')) sb.Append(',');
                }

                var hotKeyStr = keyboard.ReplaceAlias(sb.ToString());

                var hotkey =  HotKey.Parse(hotKeyStr);
                hotKeys.Add(hotkey);

            }



            CommandA = commandManager.Add(keyboard.OnDown(Caps + A),
                e => { Logger.LogInformation($"{nameof(SoftwareTool)}: Caps+A triggered!!!!!!!"); });
            CommandB = (Caps + B).OnDown(e => Logger.LogWarning("Caps+B pressed!!!"));

            Logger.LogInformation(config.CurrentValue.Option2.ToString());
            RegisterCommands();
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
                if(dir.StartsWith("_")) continue;

                var fs = GetFiles(dir);
                foreach (var file in fs)
                {
                    yield return file;
                }
             
            }

            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                if(file.StartsWith("_")) continue;

                yield return Path.Combine(path, file);
            }
        }
    }
}