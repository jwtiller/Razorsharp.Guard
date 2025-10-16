using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc;
using System.CommandLine;
using System.Reflection;
using Razorsharp.Guard.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Razorsharp.Guard.CLI
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var path = new Option<string>("--path")
            {
                Required = false,
                DefaultValueFactory = _ => Directory.GetCurrentDirectory()
            };
            var root = new RootCommand("generate report") { path };
            root.TreatUnmatchedTokensAsErrors = false;
            root.SetAction(pr =>
            {
                var p = pr.GetValue(path);
                if (string.IsNullOrEmpty(p) || (!Directory.Exists(p) && !File.Exists(p)))
                {
                    Console.Error.WriteLine("Directory or assembly does not exist");
                    return 1;
                }

                bool isDirectory = Directory.Exists(p);
                bool isFile = !isDirectory;

                if (isFile && Path.GetExtension(p) != ".dll")
                {
                    Console.Error.WriteLine("File extension for path is not dll");
                    return 1;
                }

                var assemblies = new List<string>();
                if (isDirectory)
                    assemblies.AddRange(Directory.GetFiles(p, "*.dll"));
                else
                    assemblies.Add(p);



                var runtimeDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory();
                var dir = Path.GetDirectoryName(p)!;

                // legg til både runtime og prosjektets dll-er i resolveren
                var allDlls = Directory.GetFiles(runtimeDir, "*.dll")
                    .Concat(Directory.GetFiles(dir, "*.dll"))
                    .ToList();

                var resolver = new PathAssemblyResolver(allDlls);
                foreach (var assembly in assemblies)
                {
                    using var mlc = new MetadataLoadContext(resolver);
                    var asm = mlc.LoadFromAssemblyPath(assembly);
                    var controllerTypes = asm.GetTypes()
                        .Where(t => t.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    foreach (var controllerType in controllerTypes)
                    {
                    }
                }

                return 0;
            });

            return root.Parse(args).Invoke();
        }

    }
}
