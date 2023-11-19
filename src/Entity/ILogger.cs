namespace EntityDi;
public enum LogType
{
	Info = 0,
	Warning = 1,
	Error = 2,
}
public interface ILogger
{
	void LogMessage(string message, LogType type);
}
public static class LoggerExtensions
{
	public static void Log(this ILogger logger, string msg) => logger.LogMessage(msg, LogType.Info);
}