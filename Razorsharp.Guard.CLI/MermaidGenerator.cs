// Copyright (C) 2025- Razorsharp AS
// Licensed under the GNU Affero General Public License v3.0 or later (see file LICENSE).
// Commercial licenses are available: license@razorsharp.dev

using Razorsharp.Guard.Entities;
using System.Text;
using static Razorsharp.Guard.CLI.CecilDescribe;


namespace Razorsharp.Guard.CLI
{
    public static class MermaidGenerator
    {
        public static string BuildMermaidGraph(IEnumerable<ApiReport> reports)
        {
            var sb = new StringBuilder();
            sb.AppendLine("``` mermaid");
            sb.AppendLine($"    graph TD");

            foreach (var controllerGroup in reports.GroupBy(r => r.Controller))
            {
                var controllerName = Sanitize(controllerGroup.Key);
                sb.AppendLine($"        {controllerName}[{controllerGroup.Key}]");

                foreach (var report in controllerGroup)
                {
                    var methodId = $"{controllerName}_{Sanitize(report.Method)}";
                    var methodLabel = $"{report.Verb} {report.Path ?? report.Method}";
                    sb.AppendLine($"    {controllerName} --> {methodId}[({methodLabel})]");

                    var returnNodeId = $"{methodId}_{Sanitize(report.ReturnType)}";
                    var maxSensitivity = report.Classification.Any()
                        ? report.Classification.Max(c => c.SensitivityLevel)
                        : SensitivityLevel.Public;
                    var colorEmoji = SensitivityLabel(maxSensitivity);

                    sb.AppendLine($"        {methodId} --> {returnNodeId}[{report.ReturnType}: {colorEmoji}]");

                    foreach (var cls in report.Classification.Where(c => c.AttributeLevel == AttributeLevel.Property))
                    {
                        var propId = $"{returnNodeId}_{Sanitize(cls.Type)}";
                        var emoji = SensitivityLabel(cls.SensitivityLevel);
                        var propLabel = cls.Type.Split('.').Last();
                        sb.AppendLine($"        {returnNodeId} --> {propId}[{propLabel}: {emoji}]");
                    }
                }
            }

            sb.AppendLine("```");
            return sb.ToString();
        }

        private static string SensitivityLabel(SensitivityLevel level) => level switch
        {
            SensitivityLevel.Public => "Public",
            SensitivityLevel.Internal => "Internal",
            SensitivityLevel.Confidential => "Confidential",
            SensitivityLevel.Restricted => "Restricted",
            _ => "Unknown"
        };

        private static string Sanitize(string name)
        {
            if (string.IsNullOrEmpty(name)) return "unknown";
            var invalid = new[] { '.', '<', '>', '-', ' ', '(', ')', '[', ']', '{', '}', ':', ',', '/', '\\' };
            foreach (var ch in invalid)
                name = name.Replace(ch, '_');
            return name;
        }
    }
}
