﻿using System;
using System.Linq;
using System.Windows;
using Metatool.Input.MouseKeyHook.Implementation;
using Metatool.Service;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Metaseed.Metatool;

public static class Program
{
	private const string DebugFlagShort = "-d";
	private const string DebugFlagLong = "--debug";
	internal const string AdminFlagLong = "--admin";
	[STAThread]
	[System.Diagnostics.DebuggerNonUserCode]
	public static int Main(string[] args)
	{
		try
		{
			Console.WriteLine($"Is Elevated: {Context.IsElevated}");
			var shiftDown = KeyboardState.Current().IsDown(Key.Shift);
			if (!Context.IsElevated && (shiftDown || args.Contains(AdminFlagLong)))
			{
				// so you could pin metatool to the first windows task bar shortcut,
				// and use Win+Shift+1 then Alt+Y to launch as admin,
				// note: could not use Win+Alt+1, it's used to do right click on the first task bar item
				args = args.Where(i => i != AdminFlagLong).ToArray();
				return Context.Restart(0, true, args);
			}
			args = args.Where(i => i != AdminFlagLong).ToArray(); 
			
			ServiceConfig.BuildHost(args).Run();
			return 0;
		}
		catch (Exception e)
		{
			if (e is AggregateException aggregateEx)
			{
				e = aggregateEx.Flatten().InnerException;
			}
			MessageBox.Show(e.Message + e.StackTrace);

			if (e is CompilationErrorException)
			{
				// no need to write out anything as the upstream services will report that
				return 0x1;
			}

			// Be verbose (stack trace) in debug mode otherwise brief
			var error = args.Any(arg => arg == DebugFlagShort
			                            || arg == DebugFlagLong)
				? e.ToString()
				: e.GetBaseException().Message;
			Services.CommonLogger.LogError($"Error: {error}");
			return 0x1;
		}
	}

}