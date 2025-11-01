// Copyright (C) 2025- Razorsharp AS
// Licensed under the GNU Affero General Public License v3.0 or later (see file LICENSE).
// Commercial licenses are available: license@razorsharp.dev

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Razorsharp.Guard.Entities
{
    public class GuardOptions
    {
        public GuardMode GuardMode { get; set; } = GuardMode.CallbackOnly;
        public Action<ILogger, HttpContext, GuardEvent>? Callback { get; set; }
    }
}
