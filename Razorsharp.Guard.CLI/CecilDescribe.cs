// Copyright (C) 2025- Razorsharp AS
// Licensed under the GNU Affero General Public License v3.0 or later (see file LICENSE).
// Commercial licenses are available: license@razorsharp.dev

using Mono.Cecil;
using Razorsharp.Guard.Entities;
using AssemblyDefinition = Mono.Cecil.AssemblyDefinition;
using MethodDefinition = Mono.Cecil.MethodDefinition;
using TypeReference = Mono.Cecil.TypeReference;

namespace Razorsharp.Guard.CLI
{
    public static class CecilDescribe
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
        /// Reads assembly metadata using Mono.Cecil — no dependencies need to be loaded.
        /// Finds controllers, HTTP methods, routes, and return types, and emits a JSON report.
        /// </summary>
        public static List<ApiReport> DescribeAssembly(string assemblyPath, System.Reflection.Assembly asm)
        {
            if (!File.Exists(assemblyPath))
                throw new FileNotFoundException("Assembly not found", assemblyPath);

            var results = new List<ApiReport>();
            var asmDef = AssemblyDefinition.ReadAssembly(assemblyPath);

            foreach (var type in asmDef.MainModule.Types
                         .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Controller")))
            {
                foreach (var method in type.Methods.Where(m => m.IsPublic && !m.IsConstructor))
                {
                    var verb = GetHttpVerb(method);
                    if (verb == null)
                        continue;

                    var path = GetPath(method);
                    var returnType = SimplifyTypeName(method.ReturnType);


                    var classification = new List<ClassificationResult>();
                    try
                    {
                        var typeDef = CecilTypeResolver.ResolveSystemType(asmDef, returnType);
                        if (typeDef != null)
                            classification = TypeClassification.Inspect(null, typeDef);
                    }
                    catch
                    {
                    }

                    results.Add(new ApiReport
                    {
                        Controller = type.FullName,
                        Method = method.Name,
                        Verb = verb,
                        Path = path,
                        ReturnType = returnType,
                        Classification = classification
                    });
                }
            }
            return results;
        }

        private static string? GetHttpVerb(MethodDefinition method)
        {
            foreach (var attr in method.CustomAttributes)
            {
                var name = attr.AttributeType.Name;
                if (name.StartsWith("Http", StringComparison.OrdinalIgnoreCase)
                    && name.EndsWith("Attribute", StringComparison.OrdinalIgnoreCase))
                    return name.Replace("Http", "").Replace("Attribute", "").ToUpperInvariant();
            }
            return null;
        }

        private static string? GetPath(MethodDefinition method)
        {
            foreach (var attr in method.CustomAttributes)
            {
                var name = attr.AttributeType.Name;
                if (name == "RouteAttribute" || name.StartsWith("Http"))
                {
                    var arg = attr.ConstructorArguments.FirstOrDefault();
                    if (arg.Value is string s && !string.IsNullOrWhiteSpace(s))
                        return s;
                }
            }
            return null;
        }

        private static string SimplifyTypeName(TypeReference type)
        {
            if (type == null)
                return "unknown";

            if (type.FullName == "System.Void")
                return "void";

            if (type is GenericInstanceType git)
            {
                var defName = git.ElementType.FullName;

                if (defName.StartsWith("Microsoft.AspNetCore.Mvc.ActionResult") ||
                    defName.StartsWith("System.Threading.Tasks.Task") ||
                    defName.StartsWith("System.Threading.Tasks.ValueTask"))
                {
                    var inner = git.GenericArguments.FirstOrDefault();
                    return inner != null ? SimplifyTypeName(inner) : "void";
                }

                var args = string.Join(", ", git.GenericArguments.Select(SimplifyTypeName));
                return $"{git.ElementType.Name.Split('`')[0]}<{args}>";
            }

            if (type.FullName is "Microsoft.AspNetCore.Mvc.IActionResult" or "Microsoft.AspNetCore.Mvc.ActionResult")
                return "void";

            return type.FullName;
        }

    }
}
