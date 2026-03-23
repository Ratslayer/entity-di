// using System;
// using System.Collections.Generic;
//
// // public static class Log
// // {
// //     public static ILogger Logger { get; private set; }
// //     public static void BindLogger(ILogger logger) => Logger = logger;
// //     public static void Error(string message) => Logger.Error(message);
// //     public static void Info(string message) => Logger.Info(message);
// //
// //     public static void Exception(Exception e, string message)
// //         => Logger.Error(
// //             $"EXCEPTION: {message}\n" +
// //             $"{e.GetType().Name}\n" +
// //             $"{e.Message}");
// //
// //     public static bool Assert(bool value, string message)
// //     {
// //         if (!value)
// //             Error(message);
// //         return !value;
// //     }
// // }
//
// // public static class LoggerExtensions
// // {
// //     public static void LogError(this ILogger logger, string message)
// //         => logger.Log(message, LogLevel.Error);
// // }
//
// // public readonly struct LoggerPrefix : IDisposable
// // {
// //     readonly object _prefix;
// //     readonly ILogger _logger;
// //
// //     public LoggerPrefix(ILogger logger, object prefix)
// //     {
// //         _prefix = prefix;
// //         _logger = logger;
// //         _logger.AddContext(_prefix);
// //     }
// //
// //     public void Dispose()
// //     {
// //         _logger.RemovePrefix(_prefix);
// //     }
// // }
//
// // public readonly struct LoggerUnityObjectContext : IDisposable
// // {
// //     readonly UnityEngine.Object _context;
// //     readonly UnityLogger _logger;
// //
// //     public LoggerUnityObjectContext(UnityLogger logger, UnityEngine.Object context)
// //     {
// //         _context = context;
// //         _logger = logger;
// //         _logger._contexts.Add(_context);
// //     }
// //
// //     public void Dispose()
// //     {
// //         _logger._contexts.Remove(_context);
// //     }
// // }
//
// public enum LogLevel
// {
//     Info,
//     Warning,
//     Error
// }
//
// // public interface ILogger
// // {
// //     void Log(in LoggingContext context);
// // }
//
// public readonly struct LoggingContext
// {
//     public string Message { get; init; }
//     public Exception Exception { get; init; }
//     public LogLevel Level { get; init; }
// }