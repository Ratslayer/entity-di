namespace EntityDi;
public enum LogType
{
	Info = 0,
	Warning = 1,
	Error = 2,
}
public interface ILogger
{
	void Log(string message, LogType type);
}
