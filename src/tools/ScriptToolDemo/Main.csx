#r "nuget:Newtonsoft.Json/9.0.1"
#r "nuget:Automapper,9.0.0"
#r "LocalLib.dll"

#load "LocalScript.csx"
#load "https://gist.githubusercontent.com/metasong/418dde5c695ff087c59cf54255897fd2/raw/a079fa6d09716c35402d88be0ccc3fd877cf2a73/RemoteCSharpScriptTest.csx"

using AutoMapper;
using System;
using System.Threading;
using LocalLib;

Console.WriteLine(typeof(MapperConfiguration));
Console.WriteLine("Hello, we are using Automapper nuget lib");

public class ClassTest {
    public void Hello()
    {
        var localLib = new ClassInLocalLib();
        localLib.Hello();
    }
}

new ClassTest().Hello();
