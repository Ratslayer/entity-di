using System;
using System.Runtime.CompilerServices;

namespace BB
{
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
}