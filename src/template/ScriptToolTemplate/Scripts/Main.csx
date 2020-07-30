﻿#r "nuget: Metatool.Service, *"
#r "nuget: Automapper, 9.0.0"
#r "LocalLib.dll"
#load "LocalScript.csx"
#load "https://gist.githubusercontent.com/metasong/418dde5c695ff087c59cf54255897fd2/raw/a4dafc72299e91e1f1741449f484673013966169/RemoteCSharpScriptTest.csx"

using AutoMapper;
using System;
using System.Threading;
using LocalLib;
using Microsoft.Extensions.Logging;
using Metatool.Service;
using static Metatool.Service.Key;
using System.Diagnostics;

// Debugger.Break();
Console.WriteLine("Hello we are in Main.csx");
public class ClassTest
{
    public void Hello()
    {
        var localLib = new ClassInLocalLib();
        Console.WriteLine("Hello from local class");
        localLib.Hello();
    }
}
public class MetaScript : ToolBase
{
    public ICommandToken<IKeyEventArgs> token;
    public MetaScript(ICommandManager commandManager, IKeyboard keyboard) 
    {
        Logger.LogInformation("Demo script created");
        token = commandManager.Add(keyboard.OnDown(Caps + A), e =>
          {
              Logger.LogInformation("Caps+A is triggered!!!!!");
          });
    }

    public override bool OnLoaded()
    {
        Logger.LogInformation($"we are using {typeof(MapperConfiguration)} nuget lib.");
        new ClassInGist().Hello();
        LocalScript.HelloFromLocalScript();
        new ClassTest().Hello();
        return base.OnLoaded();
    }
    public override void OnUnloading()
    {
        token.Remove();
        base.OnUnloading();
    }
}
