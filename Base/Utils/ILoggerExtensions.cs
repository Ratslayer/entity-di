using System;

public static class ILoggerExtensionsMap
{
	public static void Info(this ILogger logger, string msg) => logger.Log(msg, LogLevel.Info);
	public static void Error(this ILogger logger, string msg) => logger.Log(msg, LogLevel.Error);
	public static void Warning(this ILogger logger, string msg) => logger.Log(msg, LogLevel.Warning);
	public static void LogException(this ILogger logger, Exception exception)
		=> logger.LogException(exception);
	public static bool Assert(this ILogger logger, bool condition, string msg)
	{
		if (!condition)
			logger.Error(msg);
		return condition;
	}
	public static LoggerUnityObjectContext UseUnityContext(this ILogger logger, UnityEngine.Object context)
		=> new((UnityLogger)logger, context);
	public static LoggerPrefix UseContext(this ILogger logger, object prefix)
		=> new(logger, prefix);
}
