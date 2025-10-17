using System.CommandLine;
using System.Reflection;
using System.Runtime.Loader;

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

  
                        var asm = Assembly.LoadFrom(assemblyPath);
                        var guardAsm = Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Razorsharp.Guard.dll"));
                        if (guardAsm == null)
                        {
                            Console.WriteLine("No reference to Razorsharp.Guard found.");
                            continue;
                        }


                        var describeType = guardAsm.GetType("Razorsharp.Guard.SelfDescribe");
                        var method = describeType?.GetMethod("DescribeSelf", BindingFlags.Public | BindingFlags.Static);

                        if (method == null)
                        {
                            Console.WriteLine("DescribeSelf() not found in Razorsharp.Guard.");
                            continue;
                        }

                        var json = (string)method.Invoke(null, new object?[] { asm })!;
                        Console.WriteLine(json);
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