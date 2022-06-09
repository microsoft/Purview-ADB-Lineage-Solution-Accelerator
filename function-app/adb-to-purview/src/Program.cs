using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Function.Domain.Helpers;
using Function.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Function.Domain.Middleware;

namespace TestFunc
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureLogging((context, builder) =>
                    {
                        var key = context.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
                        builder.AddApplicationInsights(key);
                    })
                .ConfigureFunctionsWorkerDefaults(workerApplication =>
                    {
                        workerApplication.UseMiddleware<ScopedLoggingMiddleware>();
                    })
                .ConfigureServices(s =>                
                    {                        
                    s.AddScoped<IHttpHelper, HttpHelper>();
                    s.AddScoped<IOlToPurviewParsingService, OlToPurviewParsingService>(); 
                    s.AddScoped<IPurviewIngestion, PurviewIngestion>();
                    s.AddScoped<IOlFilter, OlFilter>();
                    s.AddScoped<IOlConsolodateEnrich, OlConsolodateEnrich>();            
                    })
                .Build();
            host.Run();
        }
    }
}