using System;

namespace Metaseed.CSharpScript;

public class Utils
{
	public static bool GetYesNo(string prompt, bool defaultAnswer, ConsoleColor? promptColor = null, ConsoleColor? promptBgColor = null)
	{
		string str = defaultAnswer ? "[Y/n]" : "[y/N]";
		while (true)
		{
			var bg = Console.BackgroundColor;
			var fg = Console.ForegroundColor;
			Console.ForegroundColor = promptColor??fg;
			Console.BackgroundColor = promptBgColor??bg;
			Console.Write(prompt + " " + str);
			Console.ForegroundColor = fg;
			Console.BackgroundColor = bg;
			Console.Write(' ');
			string text = Console.ReadLine()?.ToLower()?.Trim();
                
			if (string.IsNullOrEmpty(text))
			{
				return defaultAnswer;
			}
			if (text == "n" || text == "no")
			{
				return false;
			}
			if (text == "y" || text == "yes")
			{
				break;
			}
			Console.WriteLine("Invalid response '" + text + "'. Please answer 'y' or 'n' or CTRL+C to exit.");
		}
		return true;
	}
}