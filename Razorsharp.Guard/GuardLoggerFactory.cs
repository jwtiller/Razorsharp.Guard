using Microsoft.Extensions.Logging;

namespace Razorsharp.Guard
{
    public class GuardLoggerFactory : ILoggerFactory
    {
        private readonly ILoggerFactory _inner;

        public GuardLoggerFactory(ILoggerFactory inner)
        {
            _inner = inner;
        }

        public void AddProvider(ILoggerProvider provider) => _inner.AddProvider(provider);

        public ILogger CreateLogger(string categoryName)
        {
            var logger = _inner.CreateLogger(categoryName);
            return GuardLoggerProxy.Create(logger);
        }

        public void Dispose() => _inner.Dispose();
    }
}