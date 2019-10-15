#r "nuget: Metatool.Plugin, 1.4.1"
#r "nuget: Automapper, 9.0.0"
#r "LocalLib.dll"
#load "LocalScript.csx"
//#load "https://gist.githubusercontent.com/metasong/418dde5c695ff087c59cf54255897fd2/raw/0ca795cd567d88818efd857d61ddd9643d4d3049/RemoteCSharpScriptTest.csx"

using AutoMapper;
using System;
using System.Threading;
using LocalLib;
using Metatool.Plugin;
using Microsoft.Extensions.Logging;
using Metatool.Command;
using Metatool.Input;
using static Metatool.Input.Key;
using System.Diagnostics;
public class ClassTest
{
    public void Hello()
    {
        var localLib = new ClassInLocalLib();
        Console.WriteLine("Hello from local class");
        localLib.Hello();
    }
}
public class MetaScript : PluginBase
{
    IRemove token;
    public MetaScript(ILogger<MetaScript> logger, ICommandManager commandManager, IKeyboard keyboard) : base(logger)
    {
        Debugger.Break();
        logger.LogInformation("Demo script created");
        token = commandManager.Add(keyboard.Down(Caps + A), e =>
          {
              logger.LogInformation("Caps+A is triggered!!!!!");
          });
    }

    public override bool OnLoaded()
    {
        Log.LogInformation($"we are using {typeof(MapperConfiguration)} nuget lib.");
        //new ClassInGist().Hello();
        new ClassTest().Hello();
        return base.OnLoaded();
    }
    public override void OnUnloading()
    {
        token.Remove();
        base.OnUnloading();
    }
}
BBc();
Console.WriteLine("Hello from local class222");