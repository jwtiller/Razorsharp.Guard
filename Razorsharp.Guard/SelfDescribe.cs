using System.Reflection;
using System.Text.Json;
using Razorsharp.Guard.Entities;

namespace Razorsharp.Guard
    {
        public static class SelfDescribe
        {
            public class ApiReport
            {
                public string Controller { get; set; } = "";
                public string Method { get; set; } = "";
                public string Verb { get; set; } = "";
                public string? Path { get; set; }
                public string ReturnType { get; set; } = "";
                public List<ClassificationResult> Classification { get; set; } = new();
            }

            /// <summary>
            /// Scans this assembly for API controllers, extracts their HTTP methods, paths and return types,
            /// runs classification analysis, and returns JSON.
            /// </summary>
            public static string DescribeSelf(string assemblyPath = null)
            {
                var asm = string.IsNullOrEmpty(assemblyPath) 
                    ? Assembly.GetEntryAssembly()
                    : Assembly.LoadFile(assemblyPath);

                var results = new List<ApiReport>();

                var controllers = asm.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Controller"))
                    .ToList();

                foreach (var controller in controllers)
                {
                    foreach (var method in controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    {
                        var verb = GetHttpVerb(method);
                        if (verb == null) continue;

                        var path = GetPath(method);
                        var returnType = SimplifyTypeName(method.ReturnType);

                        // kjør klassifisering på returtypen (hvis ikke void)
                        var classification = new List<ClassificationResult>();
                        if (method.ReturnType != typeof(void))
                        {
                            try
                            {
                                classification = TypeClassification.Inspect(null, method.ReturnType);
                            }
                            catch { /* ignorér typer som ikke kan analyseres */ }
                        }

                        results.Add(new ApiReport
                        {
                            Controller = controller.FullName ?? controller.Name,
                            Method = method.Name,
                            Verb = verb,
                            Path = path,
                            ReturnType = returnType,
                            Classification = classification
                        });
                    }
                }

                var opts = new JsonSerializerOptions { WriteIndented = true };
                return JsonSerializer.Serialize(results, opts);
            }

            private static string? GetHttpVerb(MethodInfo method)
            {
                foreach (var attr in method.GetCustomAttributes(inherit: true))
                {
                    var name = attr.GetType().Name;
                    if (name.StartsWith("Http", StringComparison.OrdinalIgnoreCase) && name.EndsWith("Attribute"))
                        return name.Replace("Http", "").Replace("Attribute", "").ToUpperInvariant();
                }
                return null;
            }

            private static string? GetPath(MethodInfo method)
            {
                // prøv å finne "Route"- eller HttpXxx-attributt med verdi
                foreach (var attr in method.GetCustomAttributes(inherit: true))
                {
                    var type = attr.GetType();
                    var name = type.Name;

                    if (name == "RouteAttribute" || name.StartsWith("Http"))
                    {
                        var prop = type.GetProperty("Template") ?? type.GetProperty("RouteTemplate");
                        var value = prop?.GetValue(attr) as string;
                        if (!string.IsNullOrEmpty(value))
                            return value;
                    }
                }
                return null;
            }

            private static string SimplifyTypeName(Type type)
            {
                if (type == typeof(void))
                    return "void";
                if (type == typeof(Task))
                    return "Task";

                if (type.IsGenericType)
                {
                    var def = type.GetGenericTypeDefinition();
                    if (def == typeof(Task<>))
                        return $"Task<{SimplifyTypeName(type.GetGenericArguments()[0])}>";
                    if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
                        return $"List<{SimplifyTypeName(type.GetGenericArguments()[0])}>";
                    return $"{type.Name.Split('`')[0]}<{string.Join(", ", type.GetGenericArguments().Select(SimplifyTypeName))}>";
                }
                return type.Name;
            }
        }
    }
