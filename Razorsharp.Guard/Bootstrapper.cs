using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(Razorsharp.Guard.Bootstrapper))]
namespace Razorsharp.Guard
{
    public class Bootstrapper : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped<ClassificationFilter>();
                services.Configure<MvcOptions>(options =>
                {
                    options.Filters.Add<ClassificationFilter>(order: int.MaxValue);
                });
            });
        }
    }
}
