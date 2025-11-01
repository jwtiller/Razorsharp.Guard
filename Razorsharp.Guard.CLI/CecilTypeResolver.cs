// Copyright (C) 2025- Razorsharp AS
// Licensed under the GNU Affero General Public License v3.0 or later (see file LICENSE).
// Commercial licenses are available: license@razorsharp.dev

using System.Reflection;
using Mono.Cecil;

public static class CecilTypeResolver
{
    public static Type? ResolveSystemType(AssemblyDefinition asmDef, string fullName)
    {
        if (asmDef == null)
            return null;

        var localType = asmDef.MainModule.GetType(fullName);
        if (localType != null)
        {
            try
            {
                var localAsm = Assembly.LoadFrom(asmDef.MainModule.FileName);
                var localRuntimeType = localAsm.GetType(fullName, throwOnError: false);
                if (localRuntimeType != null)
                    return localRuntimeType;
            }
            catch
            {
            }
        }


        var dir = Path.GetDirectoryName(asmDef.MainModule.FileName)!;
        foreach (var dll in Directory.GetFiles(dir, "*.dll"))
        {
            try
            {
                var runtimeAsm = Assembly.LoadFrom(dll);
                var foundType = runtimeAsm.GetType(fullName, throwOnError: false);
                if (foundType != null)
                    return foundType;
            }
            catch
            {
            }
        }

        return null;
    }
}
