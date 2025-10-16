using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Razorsharp.Guard.Entities
{
    public class GuardOptions
    {
        public GuardMode GuardMode { get; set; } = GuardMode.Audit;
        public Action<ILogger, HttpContext, GuardEvent>? Audit { get; set; }
    }
}
