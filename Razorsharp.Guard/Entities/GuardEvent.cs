// Copyright (C) 2025- Razorsharp AS
// Licensed under the GNU Affero General Public License v3.0 or later (see file LICENSE).
// Commercial licenses are available: license@razorsharp.dev

namespace Razorsharp.Guard.Entities
{
    public record GuardEvent
    (
        IReadOnlyList<ClassificationResult> Classifications
    );
}
