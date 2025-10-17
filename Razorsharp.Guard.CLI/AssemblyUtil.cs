using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Razorsharp.Guard.CLI
{
    internal static class AssemblyUtil
    {
        public static IEnumerable<string> GetRuntimeAssemblies()
        {
            // F.eks. C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.8\
            var runtimeDir = RuntimeEnvironment.GetRuntimeDirectory();

            // shared-folderen er to nivåer opp fra den
            // -> C:\Program Files\dotnet\shared\
            var sharedRoot = Path.GetFullPath(@$"{runtimeDir}\..\..");

            var frameworks = new[]
            {
                "Microsoft.NETCore.App",
                "Microsoft.AspNetCore.App"
            };

            var results = new List<string>();

            foreach (var fw in frameworks)
            {
                var fwDir = Path.Combine(sharedRoot, fw);
                if (!Directory.Exists(fwDir))
                    continue;

                var latest = Directory.GetDirectories(fwDir)
                    .OrderByDescending(Path.GetFileName)
                    .FirstOrDefault();

                if (latest == null)
                    continue;

                results.AddRange(Directory.GetFiles(latest, "*.dll"));
            }

            return results.Distinct(StringComparer.OrdinalIgnoreCase);
        }
    }
}
