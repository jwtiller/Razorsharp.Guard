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
