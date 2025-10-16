using System.Reflection;
using Razorsharp.Guard.Entities;

namespace Razorsharp.Guard
{
    public static class TypeClassification
    {
        public static List<ClassificationResult> Inspect(object instance, Type type)
        {
            var visited = new HashSet<Type>();
            var classifications = new List<ClassificationResult>();
            InspectTypeRecursive(instance, type, classifications, visited, 0, null);
            return classifications;
        }

        private static void InspectTypeRecursive(
            object instance,
            Type type,
            List<ClassificationResult> classifications,
            HashSet<Type> visited,
            int depth,
            Type? parentType)
        {
            if (type == null || visited.Contains(type))
                return;

            visited.Add(type);

            // class-level attribute
            var classMaxSensitivity = type.GetCustomAttributes(typeof(ClassificationAttribute))
                .Cast<ClassificationAttribute>()
                .MaxBy(x => x.SensitivityLevel);

            var propertiesMaxSensitivity = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.GetCustomAttribute<ClassificationAttribute>()?.SensitivityLevel
                            ?? SensitivityLevel.Restricted)
                .DefaultIfEmpty(SensitivityLevel.Restricted)
                .Max();

            classifications.Add(new ClassificationResult
            {
                Type = type.FullName,
                ParentType = parentType?.FullName,
                SensitivityLevel = classMaxSensitivity?.SensitivityLevel ?? propertiesMaxSensitivity,
                Reason = classMaxSensitivity?.Reason,
                Depth = depth,
                AttributeLevel = AttributeLevel.Class
            });

            var classSensitivity = classMaxSensitivity?.SensitivityLevel ?? SensitivityLevel.Restricted;

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var propAttribute = prop
                    .GetCustomAttributes(typeof(ClassificationAttribute))
                    .Cast<ClassificationAttribute>()
                    .MaxBy(x => x.SensitivityLevel);

                classifications.Add(new ClassificationResult
                {
                    Type = $"{type.FullName}.{prop.Name}",
                    ParentType = parentType?.FullName,
                    SensitivityLevel = propAttribute?.SensitivityLevel ?? classSensitivity,
                    Reason = propAttribute?.Reason,
                    Depth = depth,
                    AttributeLevel = AttributeLevel.Property
                });

                var propType = prop.PropertyType;

                if (propType.IsInterface)
                {
                    var propValue = prop.GetValue(instance);
                    var propActualType = propValue?.GetType();
                    if (propActualType != null)
                        InspectTypeRecursive(instance, propActualType, classifications, visited, depth + 1, propType);
                }

                if (propType == typeof(string)
                    || propType.IsPrimitive
                    || propType == typeof(DateTime)
                    || propType == typeof(TimeSpan))
                    continue;

                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(propType) &&
                    propType.IsGenericType)
                {
                    var elementType = propType.GetGenericArguments()[0];
                    InspectTypeRecursive(instance, elementType, classifications, visited, depth + 1, type);
                }
                else
                {
                    InspectTypeRecursive(instance, propType, classifications, visited, depth + 1, type);
                }
            }
        }
    }
}