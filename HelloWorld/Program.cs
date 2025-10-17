using Razorsharp.Guard.Entities;
using Razorsharp.Guard.Infrastructure;
using Razorsharp.Guard.Razorsharp.Guard;

namespace HelloWorld
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.AddProvider(new FileLoggerProvider("razosharp-guard-audit.log"));

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddRazorsharpGuard((options) =>
            {
                options.GuardMode = GuardMode.ThrowException;
                options.Audit = (logger, httpContext, evt) =>
                {
                    var user = httpContext.User?.Identity?.Name ?? "anonymous";
                    var path = httpContext.Request.Path;
                    var method = httpContext.Request.Method;
                    var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    var worst = evt.Classifications
                        .OrderByDescending(c => c.SensitivityLevel)
                        .FirstOrDefault();

                    logger.LogWarning(
                        "Sensitive data access detected. User={User}, Path={Method} {Path}, IP={IP}, MaxLevel={Level}, Reason={Reason}, Types=[{Types}]",
                        user,
                        method,
                        path,
                        ip,
                        worst?.SensitivityLevel,
                        worst?.Reason ?? "n/a",
                        string.Join(", ", evt.Classifications.Select(c => c.Type))
                    );
                };
            });

            var description = SelfDescribe.DescribeSelf();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
