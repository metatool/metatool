#r "nuget: Automapper, 9.0.0"
#r "LocalLib.dll"
#load "LocalScript.csx"
#load "https://gist.githubusercontent.com/metasong/418dde5c695ff087c59cf54255897fd2/raw/a4dafc72299e91e1f1741449f484673013966169/RemoteCSharpScriptTest.csx"

using AutoMapper;
using LocalLib;
using System;
using System.Threading;
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

new ClassInGist().Hello();
LocalScript.HelloFromLocalScript();
new ClassTest().Hello();

