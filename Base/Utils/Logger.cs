using BB;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public static class Log
{
    public static ILogger Logger { get; private set; }
    public static void BindLogger(ILogger logger) => Logger = logger;
    public static void Error(string message) => Logger.Error(message);
    public static void Info(string message) => Logger.Info(message);
    public static void Exception(Exception e, string message)
        => Logger.Error(
            $"EXCEPTION: {message}\n" +
            $"{e.GetType().Name}\n" +
            $"{e.Message}");
    public static bool Assert(bool value, string message)
    {
        if (!value)
            Error(message);
        return !value;
    }
}
public static class LoggerExtensions
{
    public static void LogError(this ILogger logger, string message)
        => logger.Log(message, LogLevel.Error);
}
public readonly struct LoggerPrefix : IDisposable
{
    readonly object _prefix;
    readonly ILogger _logger;
    public LoggerPrefix(ILogger logger, object prefix)
    {
        _prefix = prefix;
        _logger = logger;
        _logger.AddContext(_prefix);
    }

    public void Dispose()
    {
        _logger.RemovePrefix(_prefix);
    }
}
public readonly struct LoggerUnityObjectContext : IDisposable
{
    readonly UnityEngine.Object _context;
    readonly UnityLogger _logger;
    public LoggerUnityObjectContext(UnityLogger logger, UnityEngine.Object context)
    {
        _context = context;
        _logger = logger;
        _logger._contexts.Add(_context);
    }

    public void Dispose()
    {
        _logger._contexts.Remove(_context);
    }
}
public sealed class UnityLogger : ILogger
{
    public readonly List<UnityEngine.Object> _contexts = new();
    readonly List<object>
        _prefixes = new(),
        _singleUsePrefixes = new();
    public void Log(string msg, LogLevel level)
    {
        var fullMessage = GetCurrentMessage(msg, false, null);
        var context = _contexts.LastOrDefault();
        switch (level)
        {
            case LogLevel.Error:
                Debug.LogError(fullMessage, context);
                break;
            case LogLevel.Warning:
                Debug.LogWarning(fullMessage, context);
                break;
            default:
                Debug.Log(fullMessage, context);
                break;
        }
    }
    public void LogException(Exception exception)
    {
        Debug.LogException(exception);
    }
    public void AddContext(object prefix) => _prefixes.Add(prefix);
    public void RemovePrefix(object prefix)
    {
        _singleUsePrefixes.Add(prefix);
        _prefixes.Remove(prefix);
    }
    string GetCurrentMessage(string msg, bool appendSingleUse, string color)
    {
        //build message
        using var builder = PooledStringBuilder.GetPooled();
        if (color is not null)
            builder.Append($"<color={color}>");
        AppendPrefixes(_prefixes);
        if (appendSingleUse)
            AppendPrefixes(_singleUsePrefixes);
        _singleUsePrefixes.Clear();
        if (msg is not null)
            builder.Append($" {msg}");
        if (color is not null)
            builder.Append("</color>");
        var fullMessage = builder.ToString();
        return fullMessage;
        void AppendPrefixes(List<object> prefixes)
        {
            foreach (var i in -prefixes.Count)
            {
                var prefix = prefixes[i];
                var prefixName = prefix is null ? "N/A" : prefix.ToString();
                builder.Append($"[{prefixName}]");
            }
        }
    }


}
public enum LogLevel
{
    Info,
    Warning,
    Error
}
public interface ILogger
{
    void Log(string msg, LogLevel level);
    void LogException(Exception exception);
    void AddContext(object prefix);
    void RemovePrefix(object prefix);
}