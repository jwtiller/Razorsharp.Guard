// Copyright (C) 2025- Razorsharp AS
// Licensed under the GNU Affero General Public License v3.0 or later (see file LICENSE).
// Commercial licenses are available: license@razorsharp.dev

using Razorsharp.Guard.Entities;

namespace Razorsharp.Guard
{

    public class RazorsharpGuardException : Exception
    {
        public IReadOnlyList<ClassificationResult> Evaluations { get; }

        public RazorsharpGuardException(string message,
            IReadOnlyList<ClassificationResult> evaluations)
            : base(message)
        {
            Evaluations = evaluations;
        }
    }
}
