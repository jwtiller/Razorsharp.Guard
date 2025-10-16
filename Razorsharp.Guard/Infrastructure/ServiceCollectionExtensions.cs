using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Razorsharp.Guard.Entities;

namespace Razorsharp.Guard.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRazorsharpGuard(
            this IServiceCollection services,
            Action<GuardOptions>? configure = null)
        {
            var options = new GuardOptions();
            configure?.Invoke(options);

            services.AddSingleton(options);

            services.AddScoped<ClassificationFilter>();
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add<ClassificationFilter>(order: int.MaxValue);
            });

            return services;
        }
    }
}
