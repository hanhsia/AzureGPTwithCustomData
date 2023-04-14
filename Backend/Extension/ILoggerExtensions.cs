using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.Extensions.Logging
{
    public static class ILoggerExtensions
    {
        public static void Enter(
            this ILogger logger,
            params object[] args)
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                string? methodName = "Unknown";
                string? className = "Unknown";
                var method = new StackFrame(1, false).GetMethod();
                var generatedType = method?.DeclaringType;
                var originalType = generatedType?.DeclaringType;

                if (originalType == null)
                {
                    //sync method
                    className = generatedType?.FullName;
                    methodName = method?.Name;
                }
                else
                {
                    //async method
                    className = originalType?.FullName;
                    IEnumerable<MethodInfo> matchingMethods =
                    from methodInfo in originalType?.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    let attr = methodInfo.GetCustomAttribute<AsyncStateMachineAttribute>()
                    where attr != null && attr.StateMachineType == generatedType
                    select methodInfo;
                    if (matchingMethods.Count() > 0)
                    {
                        MethodInfo foundMethod = matchingMethods.Single();
                        methodName = foundMethod.Name;
                    }
                }

                if (args.Count() == 0)
                {
                    logger.LogTrace("Entering {@0} {@1}", className, methodName);
                }
                else
                {
                    logger.LogTrace("Entering {@0} {@1} with args:{@2}", className, methodName, args);
                }
            }
        }

        public static void Exit(
            this ILogger logger,
            params object[] args)
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                string? methodName = "Unknown";
                string? className = "Unknown";
                var method = new StackFrame(1, false).GetMethod();
                var generatedType = method?.DeclaringType;
                var originalType = generatedType?.DeclaringType;

                if (originalType == null)
                {
                    //sync method
                    className = generatedType?.FullName;
                    methodName = method?.Name;
                }
                else
                {
                    //async method
                    className = originalType.FullName;
                    IEnumerable<MethodInfo> matchingMethods =
                    from methodInfo in originalType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    let attr = methodInfo.GetCustomAttribute<AsyncStateMachineAttribute>()
                    where attr != null && attr.StateMachineType == generatedType
                    select methodInfo;
                    if (matchingMethods.Count() > 0)
                    {
                        MethodInfo foundMethod = matchingMethods.Single();
                        methodName = foundMethod.Name;
                    }
                }

                if (args.Count() == 0)
                {
                    logger.LogTrace("Exiting {@0} {@1}", className, methodName);
                }
                else
                {
                    logger.LogTrace("Exiting {@0} {@1} with results:{@2}", className, methodName, args);
                }
            }
        }
    }
}