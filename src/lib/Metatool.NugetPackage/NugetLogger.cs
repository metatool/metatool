using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NuGet.Common;
using ILogger = NuGet.Common.ILogger;
using LogLevel = NuGet.Common.LogLevel;

namespace Metatool.NugetPackage;

internal class NugetLogger: ILogger
{
	readonly Microsoft.Extensions.Logging.ILogger _logger;

	public NugetLogger(Microsoft.Extensions.Logging.ILogger logger)
	{
		_logger = logger;
	}
	public void LogDebug(string data)
	{
		_logger.LogDebug(data);
	}

	public void LogVerbose(string data)
	{
		_logger.LogTrace(data);
	}

	public void LogInformation(string data)
	{
		_logger.LogInformation(data);
	}

	public void LogMinimal(string data)
	{
		_logger.LogDebug(data);
	}

	public void LogWarning(string data)
	{
		_logger.LogWarning(data);
	}

	public void LogError(string data)
	{
		_logger.LogError(data);
	}

	public void LogInformationSummary(string data)
	{
		_logger.LogInformation(data);
	}

	public void Log(LogLevel level, string data)
	{
		_logger.Log((Microsoft.Extensions.Logging.LogLevel)level, data);
	}

	public Task LogAsync(LogLevel level, string data)
	{
		return Task.Run(()=>Log(level, data));
	}

	public void Log(ILogMessage message)
	{
		_logger.Log((Microsoft.Extensions.Logging.LogLevel)message.Level, $"{message.Time}: Code-{message.Code}, {message.Message} ({message.WarningLevel} - {message.ProjectPath})");
	}

	public Task LogAsync(ILogMessage message)
	{
		return Task.Run(() => Log(message));
	}
}