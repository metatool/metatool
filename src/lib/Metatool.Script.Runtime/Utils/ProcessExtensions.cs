﻿using System.Diagnostics;

namespace Metatool.Script.Runtime;

internal static class ProcessExtensions
{
	public static bool IsAlive(this Process process)
	{
		try
		{
			return !process.HasExited;
		}
		catch
		{
			return false;
		}
	}
}