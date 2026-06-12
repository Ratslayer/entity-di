using System;

namespace BB
{
    public interface ILoggerScope
    {
        void Info(string msg);
        void Warning(string msg);
        void Error(string msg);
        void Exception(Exception ex, string message = null);
        void AddToScope(string key, object value);
    }
}