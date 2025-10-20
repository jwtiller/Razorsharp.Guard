using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Razorsharp.Guard.Entities;

namespace Razorsharp.Guard
{
    public class ClassificationFilter : IResultFilter
    {
        private static readonly ConcurrentDictionary<Type, List<ClassificationResult>> _cache = new();

        public readonly List<ClassificationResult> Classifications = new();
        private readonly GuardOptions _guardOptions;
        private readonly ILogger<ClassificationFilter> _logger;

        public ClassificationFilter(GuardOptions? guardOptions = null, ILogger<ClassificationFilter>? logger = null)
        {
            _guardOptions = guardOptions ?? new();
            _logger = logger;

            if (_guardOptions.Callback != null && _logger == null)
                throw new InvalidOperationException(
                    $"Razorsharp Guard has auditing enabled in options, but no logger is registered in the dependency injection container.");
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is not ObjectResult objectResult || objectResult.Value == null)
                return;

            var type = objectResult.Value.GetType();

            if (_cache.TryGetValue(type, out var cached))
            {
                Classifications.AddRange(cached);
                return;
            }

            try
            {
                var inspected = TypeClassification.Inspect(objectResult.Value, type);
                Classifications.AddRange(inspected);
                Evaluate(context);
            }
            finally
            {
                _cache.TryAdd(type, Classifications.ToList());
            }
        }

        public void OnResultExecuted(ResultExecutedContext context) { }

        private void Evaluate(ResultExecutingContext context)
        {
            var filteredClassifications = Classifications
                .Where(c => c.SensitivityLevel > SensitivityLevel.Public)
                .ToList();

            if (!filteredClassifications.Any())
                return;

            _guardOptions.Callback?.Invoke(_logger!, context.HttpContext, new GuardEvent(filteredClassifications));

            if (_guardOptions.GuardMode == GuardMode.ThrowExceptionAndCallback)
                throw new RazorsharpGuardException(
                    "Razorsharp Guard blocked response: data classified above 'Public' was detected.",
                    Classifications);
        }
    }
}
