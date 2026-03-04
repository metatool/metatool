using System;

namespace Metatool.Core;

public static class F
{
	/// <summary>
	/// usage: to scope as local variables that just used by one function, are make sure the var is not used by other function members of the class.
	/// using static Metatool.Core.F;
	/// var Func = Run<Action>(()=> {
	///     var localVar = 1; // var just used by Func
	/// 	return () => {
	///         Console.WriteLine(localVar);
	///    };
	/// });
	/// </summary>
    public static T Run<T>(Func<T> f) => f();
    public static void Run(Action f) => f();
}