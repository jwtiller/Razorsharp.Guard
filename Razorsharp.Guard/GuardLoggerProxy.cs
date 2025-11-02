using Microsoft.Extensions.Logging;
using Razorsharp.Guard.Entities;
using System.Reflection;

namespace Razorsharp.Guard
{
    public class GuardLoggerProxy : DispatchProxy
    {
        private ILogger _inner;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod == null)
                return null;

            if (targetMethod.Name.StartsWith("Log"))
            {
                RedactSensitiveData(args);
            }

            return targetMethod.Invoke(_inner, args);
        }

        private static void RedactSensitiveData(object?[]? args)
        {
            if (args == null || args.Length < 3)
                return;
 
            var state = args[2]; // "state" in ILogger.Log<TState>

            if (state is IEnumerable<KeyValuePair<string, object?>> kvps)
            {
                foreach (var kv in kvps)
                {
                    var value = kv.Value;
                    if (value == null)
                        continue;

                    RedactObject(value);
                }
            }
        }

        private static void RedactObject(object obj)
        {
            var type = obj.GetType();
            if (type.IsPrimitive || type == typeof(string))
                return;

            // TODO: TypeClassification.Inspect(obj, type);

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {   
                var attr = prop.GetCustomAttribute<ClassificationAttribute>() ?? new RestrictedAttribute("default");
                if (attr != null && attr.SensitivityLevel > SensitivityLevel.Public)
                {
                    try
                    {
                        if (prop.PropertyType == typeof(string))
                            prop.SetValue(obj, "******");
                        else
                            prop.SetValue(obj, default);
                    }
                    catch
                    {
                    }
                }
                else
                {
                    // recursive
                    var nested = prop.GetValue(obj);
                    if (nested != null && !prop.PropertyType.IsPrimitive && prop.PropertyType != typeof(string))
                        RedactObject(nested);
                }
            }
        }


        public static ILogger Create(ILogger inner)
        {
            var proxy = Create<ILogger, GuardLoggerProxy>();
            ((GuardLoggerProxy)(object)proxy)._inner = inner;
            return proxy;
        }
    }
}
