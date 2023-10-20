Directly run c# code as script, we can use it to write script plugin for MetaTool
## Example
```csharp
// support using nuget pakcage
#r "nuget: Automapper, 9.0.0"
// support using local dll
#r "LocalLib.dll"
// support using local script in directory
#load "LocalScript.csx"
// support using remote script with url, i.e., gist
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

```
## Usage
**To init scripts template in directory**
> it will add vscode config to let you debug the scripts with vscode
`cs init [-d dir]`

** to run script**

` cs <scriptPath> [-n LocalAssembly.dll] [-- arg0 arg1...]`
