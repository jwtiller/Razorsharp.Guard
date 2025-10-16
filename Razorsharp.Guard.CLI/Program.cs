using System.CommandLine;

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

                var files = new List<string>();
                if (isDirectory)
                    files.AddRange(Directory.GetFiles(p, "*.dll"));
                else
                    files.Add(p);
                
                return 0;
            });

            return root.Parse(args).Invoke();
        }

    }
}
