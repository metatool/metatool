#r "nuget:Metatool.Plugin,1.2.0"
#r "nuget:Newtonsoft.Json/9.0.1"
#r "nuget:Automapper,9.0.0"
#r "nuget:Microsoft.Extensions.Logging.Abstractions,3.0.0-preview8.19405.4"
#r "LocalLib.dll"
#load "LocalScript.csx"
#load "https://gist.githubusercontent.com/metasong/418dde5c695ff087c59cf54255897fd2/raw/0ca795cd567d88818efd857d61ddd9643d4d3049/RemoteCSharpScriptTest.csx"

using AutoMapper;
using System;
using System.Threading;
using LocalLib;
using Metatool.Plugin;
using Microsoft.Extensions.Logging;

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
    public MetaScript(ILogger<MetaScript> logger) : base(logger)
    {

    }
    public override bool Init()
    {
        Log.LogInformation($"we are using {typeof(MapperConfiguration)} nuget lib.");
        new ClassInGist().Hello();
        new ClassTest().Hello();
        return base.Init();

    }
    public override void OnUnloading()
    {
        base.OnUnloading();

    }
}
