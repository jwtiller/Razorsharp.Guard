using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Razorsharp.Guard.Entities;

namespace Razorsharp.Guard
{
    public class ClassificationFilter : IResultFilter
    {
        private static readonly ConcurrentDictionary<Type, List<ClassificationResult>> _cache = new();


        private readonly HashSet<Type> _visited = new();
        public readonly List<ClassificationResult> Classifications = new();
        private readonly GuardOptions _guardOptions;
        private readonly ILogger<ClassificationFilter> _logger;

        public ClassificationFilter(GuardOptions? guardOptions = null, ILogger<ClassificationFilter> logger = null)
        {
            _guardOptions = guardOptions ?? new();
            _logger = logger;

            if (_guardOptions.Audit != null && _logger == null)
                throw new InvalidOperationException($"Razorsharp Guard has auditing enabled in options, but no logger is registered in the dependency injection container.");
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is not ObjectResult objectResult || objectResult.Value == null)
                return;

            if (_cache.TryGetValue(objectResult.Value.GetType(), out var cached))
            {
                Classifications.AddRange(cached);
                return;
            }

            try
            {
                InspectType(objectResult.Value, objectResult.Value.GetType());
                Evaluate(context);
            }
            finally
            {
                _cache.TryAdd(objectResult.Value.GetType(), Classifications);
            }

           
        }

        public void OnResultExecuted(ResultExecutedContext context) { }

        private void Evaluate(ResultExecutingContext context)
        {
            var filteredClassifications = Classifications.Where(c => c.SensitivityLevel > SensitivityLevel.Public).ToList();

            if (filteredClassifications.Any())
            {
                if (_logger != null)
                {
                    _guardOptions.Audit?.Invoke(_logger, context.HttpContext, new GuardEvent(filteredClassifications));
                }

                if (_guardOptions.GuardMode == GuardMode.ThrowException)
                    throw new RazorsharpGuardException("Razorsharp Guard blocked response: data classified above 'Public' was detected.", Classifications);

            }

        }

        private void InspectType(object instance, Type type, int depth = 0, Type? parentType = null)
        {
            if (type == null || _visited.Contains(type))
                return;

            _visited.Add(type);

            // class attribute
            var classMaxSensitivity = type.GetCustomAttributes(typeof(ClassificationAttribute))
                .Cast<ClassificationAttribute>()
                .MaxBy(x => x.SensitivityLevel);


            var propertiesMaxSensitivity = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.GetCustomAttribute<ClassificationAttribute>()?.SensitivityLevel
                 ?? SensitivityLevel.Restricted)
                .Distinct()
                .Cast<SensitivityLevel?>()
                .Max();


                Classifications.Add(new()
                {
                    Type = type.FullName,
                    ParentType = parentType?.FullName,
                    SensitivityLevel = classMaxSensitivity?.SensitivityLevel ?? (propertiesMaxSensitivity ?? SensitivityLevel.Restricted),
                    Reason = classMaxSensitivity?.Reason,
                    Depth = depth,
                    AttributeLevel = AttributeLevel.Class
                });

            // properties
            var classSensitivity = classMaxSensitivity?.SensitivityLevel ?? SensitivityLevel.Restricted;
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // attributes on properties
                var propAttribute = prop.GetCustomAttributes(typeof(ClassificationAttribute)).Cast<ClassificationAttribute>().MaxBy(x => x.SensitivityLevel);
                
                Classifications.Add(new()
                {
                    Type = $"{type.FullName}.{prop.Name}",
                    ParentType = parentType?.FullName,
                    SensitivityLevel = propAttribute?.SensitivityLevel ?? classSensitivity,
                    Reason = propAttribute?.Reason,
                    Depth = depth,
                    AttributeLevel = AttributeLevel.Property
                });

                // recursive
                var propType = prop.PropertyType;

                if (propType.IsInterface)
                {
                    var propValue = prop.GetValue(instance);
                    var propActualType = propValue?.GetType();

                    if (propActualType != null)
                        InspectType(instance, propActualType, depth + 1, propType);
                }

                if (propType == typeof(string)
                    || propType.IsPrimitive 
                    || propType == typeof(DateTime)
                    || propType == typeof(TimeSpan))
                    continue;

                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(propType) &&
                    propType.IsGenericType)
                {
                    // if generic as List
                    var elementType = propType.GetGenericArguments()[0];
                    InspectType(instance, elementType, depth+1, type);
                }
                else
                {
                    InspectType(instance, propType, depth+1, type);
                }
            }
        }
    }

}
