// Copyright (C) 2025- Razorsharp AS
// Licensed under the GNU Affero General Public License v3.0 or later (see file LICENSE).
// Commercial licenses are available: license@razorsharp.dev

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Razorsharp.Guard.Entities;

namespace Razorsharp.Guard.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRazorsharpLoggerRedaction(this IServiceCollection services)
        {
            services.Decorate<ILoggerFactory>((inner, _) => new GuardLoggerFactory(inner));
            return services;
        }

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
