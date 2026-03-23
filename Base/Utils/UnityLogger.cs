using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BB.Di;
using UnityEngine;

namespace BB
{
    public static class LoggerConstants
    {
        public const string EntityContextKey = "entity";
        public const string ClassContextKey = "class";
        public const string MethodContextKey = "method";
    }

    public sealed class UnityLoggerScope : PooledObject<UnityLoggerScope>, ILoggerScope
    {
        public const string UnityObjectContextKey = "unity_object";
        private UnityEngine.Object _context;
        private readonly Dictionary<string, object> _values = new();

        public void Info(string msg)
        {
            Debug.Log(ProcessMessage(msg), _context);
            Dispose();
        }

        public void Warning(string msg)
        {
            Debug.LogWarning(ProcessMessage(msg), _context);
            Dispose();
        }

        public void Error(string msg)
        {
            Debug.LogError(ProcessMessage(msg), _context);
            Dispose();
        }

        public void Exception(Exception ex, string msg)
        {
            Debug.LogError(ProcessMessage(msg), _context);
            Debug.LogException(ex, _context);
            Dispose();
        }

        public void AddToScope(string key, object value)
        {
            if (key is UnityObjectContextKey && value is UnityEngine.Object unityObject)
                _context = unityObject;
            _values[key] = value;
        }

        public override void Dispose()
        {
            base.Dispose();
            _context = null;
            _values.Clear();
        }

        private string ProcessMessage(string msg)
        {
            using var builder = PooledStringBuilder.GetPooled();

            Append(LoggerConstants.EntityContextKey);
            Append(LoggerConstants.ClassContextKey);
            Append(LoggerConstants.MethodContextKey);

            builder.Append(" ");
            builder.Append(msg);

            return builder.ToString();

            void Append(string key)
            {
                if (!_values.TryGetValue(key, out var value))
                    return;
                if (!builder.Empty)
                    builder.Append(":");
                builder.Append(value.ToString());
            }
        }
    }

    public interface ILoggerScope
    {
        void Info(string msg);
        void Warning(string msg);
        void Error(string msg);
        void Exception(Exception ex, string message = null);
        void AddToScope(string key, object value);
    }

    public interface ILoggerScopeFactory
    {
        ILoggerScope GetScope();
        ILoggerScope GetScopeFromEntity(IEntity entity);
    }

    public sealed class UnityLoggerScopeFactory : ILoggerScopeFactory
    {
        public ILoggerScope GetScope()
        {
            return UnityLoggerScope.GetPooled();
        }

        public ILoggerScope GetScopeFromEntity(IEntity entity)
        {
            var scope = UnityLoggerScope.GetPooled();

            scope.AddToScope(LoggerConstants.EntityContextKey, entity.Name);
            if (entity.Has(out Root root))
                scope.AddToScope(UnityLoggerScope.UnityObjectContextKey, root.GameObject);
            if (entity.Has(out Root2D root2D))
                scope.AddToScope(UnityLoggerScope.UnityObjectContextKey, root2D.GameObject);

            return scope;
        }
    }

    public static class UnityLoggerScopeExtensions
    {
        public static ILoggerScope WithUnityObject(this ILoggerScope scope, UnityEngine.Object unityObject)
        {
            scope.AddToScope(UnityLoggerScope.UnityObjectContextKey, unityObject);
            return scope;
        }
    }

    public static class LoggerScopeExtensions
    {
        public static ILoggerScope WithClass(this ILoggerScope scope, Type type)
        {
            scope.AddToScope(LoggerConstants.ClassContextKey, type.Name);
            return scope;
        }

        public static ILoggerScope WithMethod(this ILoggerScope scope, [CallerMemberName] string methodName = null)
        {
            scope.AddToScope(LoggerConstants.MethodContextKey, methodName);
            return scope;
        }

        public static bool NotTrue(this ILoggerScope scope, bool value, string errorMessage)
        {
            if (value)
            {
                scope.TryDispose();
                return false;
            }

            scope.Error(errorMessage);
            return true;
        }
    }

    // public sealed class UnityLogger : ILogger
    // {
    //     public readonly List<UnityEngine.Object> _contexts = new();
    //
    //     readonly List<object>
    //         _prefixes = new(),
    //         _singleUsePrefixes = new();
    //
    //     public void Log(string msg, LogLevel level)
    //     {
    //         var fullMessage = GetCurrentMessage(msg, false, null);
    //         var context = _contexts.LastOrDefault();
    //         switch (level)
    //         {
    //             case LogLevel.Error:
    //                 Debug.LogError(fullMessage, context);
    //                 break;
    //             case LogLevel.Warning:
    //                 Debug.LogWarning(fullMessage, context);
    //                 break;
    //             default:
    //                 Debug.Log(fullMessage, context);
    //                 break;
    //         }
    //     }
    //
    //     public void LogException(Exception exception)
    //     {
    //         Debug.LogException(exception);
    //     }
    //
    //     public void AddContext(object prefix) => _prefixes.Add(prefix);
    //
    //     public void RemovePrefix(object prefix)
    //     {
    //         _singleUsePrefixes.Add(prefix);
    //         _prefixes.Remove(prefix);
    //     }
    //
    //     string GetCurrentMessage(string msg, bool appendSingleUse, string color)
    //     {
    //         //build message
    //         using var builder = PooledStringBuilder.GetPooled();
    //         if (color is not null)
    //             builder.Append($"<color={color}>");
    //         AppendPrefixes(_prefixes);
    //         if (appendSingleUse)
    //             AppendPrefixes(_singleUsePrefixes);
    //         _singleUsePrefixes.Clear();
    //         if (msg is not null)
    //             builder.Append($" {msg}");
    //         if (color is not null)
    //             builder.Append("</color>");
    //         var fullMessage = builder.ToString();
    //         return fullMessage;
    //
    //         void AppendPrefixes(List<object> prefixes)
    //         {
    //             foreach (var i in -prefixes.Count)
    //             {
    //                 var prefix = prefixes[i];
    //                 var prefixName = prefix is null ? "N/A" : prefix.ToString();
    //                 builder.Append($"[{prefixName}]");
    //             }
    //         }
    //     }
    //
    //
    //     public void Log(in LoggingContext context)
    //     {
    //     }
    // }
}