#r "nuget:Metaseed.MetaPlugin"
#r "nuget:Newtonsoft.Json/9.0.1"
#r "nuget:Automapper,9.0.0"
#r "LocalLib.dll"
#r "nuget:Microsoft.Extensions.Logging.Abstractions,3.0.0-preview8.19405.4"
#load "LocalScript.csx"
#load "https://gist.githubusercontent.com/metasong/418dde5c695ff087c59cf54255897fd2/raw/a079fa6d09716c35402d88be0ccc3fd877cf2a73/RemoteCSharpScriptTest.csx"

using AutoMapper;
using System;
using System.Threading;
using LocalLib;
using Metaseed.MetaPlugin;
using Microsoft.Extensions.Logging;

public class ClassTest
{
    public void Hello()
    {
        var localLib = new ClassInLocalLib();
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
        Console.WriteLine(typeof(MapperConfiguration));
        Console.WriteLine("Hello, we are using Automapper nuget lib");
        new ClassTest().Hello();
        return base.Init();

    }
    public override void OnUnloading()
    {

        base.OnUnloading();

    }
}
