// Copyright (C) 2025- Razorsharp AS
// Licensed under the GNU Affero General Public License v3.0 or later (see file LICENSE).
// Commercial licenses are available: license@razorsharp.dev

namespace Razorsharp.Guard.Entities
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public abstract class ClassificationAttribute : Attribute
    {
        public string? Reason { get; }

        protected ClassificationAttribute(string? reason = null)
        {
            Reason = reason;
        }

        public abstract SensitivityLevel SensitivityLevel { get; }
    }

    public enum SensitivityLevel
    {
        Public = 0,
        Internal = 1,
        Confidential = 2,
        Restricted = 3
    }

    public sealed class PublicAttribute : ClassificationAttribute
    {
        public PublicAttribute() : base() { }
        public override SensitivityLevel SensitivityLevel => SensitivityLevel.Public;
    }

    public sealed class InternalAttribute : ClassificationAttribute
    {
        public InternalAttribute(string reason) : base(reason) { }
        public override SensitivityLevel SensitivityLevel => SensitivityLevel.Internal;
    }

    public sealed class ConfidentialAttribute : ClassificationAttribute
    {
        public ConfidentialAttribute(string reason) : base(reason) { }
        public override SensitivityLevel SensitivityLevel => SensitivityLevel.Confidential;
    }

    public sealed class RestrictedAttribute : ClassificationAttribute
    {
        public RestrictedAttribute(string reason) : base(reason) { }
        public override SensitivityLevel SensitivityLevel => SensitivityLevel.Restricted;
    }

}
