using System.CommandLine;
using System.Reflection;
using System.Text.Json;

namespace Razorsharp.Guard.CLI
{
    internal class Program
    {
        static int Main(string[] args)
        {
            var path = new Option<string>("--path")
            {
                Required = false,
                DefaultValueFactory = _ => Directory.GetCurrentDirectory()
            };

            var root = new RootCommand("Generate Razorsharp Guard report") { path };
            root.TreatUnmatchedTokensAsErrors = false;

            root.SetAction(pr =>
            {
                var p = pr.GetValue(path);

                if (string.IsNullOrEmpty(p) || (!Directory.Exists(p) && !File.Exists(p)))
                {
                    Console.Error.WriteLine("Directory or assembly does not exist.");
                    return 1;
                }

                bool isDirectory = Directory.Exists(p);
                bool isFile = !isDirectory;

                if (isFile && Path.GetExtension(p) != ".dll")
                {
                    Console.Error.WriteLine("Path must be a .dll file.");
                    return 1;
                }

                var assemblies = isDirectory
                    ? Directory.GetFiles(p, "*.dll").ToList()
                    : new List<string> { p };

                Console.WriteLine($"Found {assemblies.Count} assemblies.");

                foreach (var assemblyPath in assemblies)
                {
                    try
                    {
                        Console.WriteLine($"Scanning: {Path.GetFileName(assemblyPath)}");
                        var result = CecilDescribe.DescribeAssembly(assemblyPath, Assembly.LoadFrom(assemblyPath));
                        var mermaid = MermaidGenerator.BuildMermaidGraph(result);

                        var reportId = Guid.NewGuid();
                        File.WriteAllText($"mermaid-{reportId}.mmd", mermaid);

                        var opts = new JsonSerializerOptions { WriteIndented = true };
                        var json = JsonSerializer.Serialize(result, opts);

                        Console.WriteLine(json);
                        File.WriteAllText($"report-{reportId}.json",json);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Failed to scan {assemblyPath}: {ex.Message}");
                    }
                }

                return 0;
            });

            return root.Parse(args).Invoke();
        }
    }
}