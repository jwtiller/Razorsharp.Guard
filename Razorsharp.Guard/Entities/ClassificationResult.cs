// Copyright (C) 2025- Razorsharp AS
// Licensed under the GNU Affero General Public License v3.0 or later (see file LICENSE).
// Commercial licenses are available: license@razorsharp.dev

namespace Razorsharp.Guard.Entities
{
    public class ClassificationResult
    {
        public string Type { get; set; }
        public string ParentType { get; set; }
        public string? Reason { get; set; }
        public SensitivityLevel SensitivityLevel { get; set; }
        public int Depth { get; set; }
        public AttributeLevel AttributeLevel { get; set; }
    }
}
