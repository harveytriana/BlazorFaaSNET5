// ======================================
// BlazorSpread.net
// ======================================
using HttpTriggerSample.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace HttpTriggerSample
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                // enable DI
                .ConfigureServices(
                    services => {
                        // enable http service
                        services.AddHttpClient();
                        // enable custom service
                        services.AddSingleton<CurrencyTools>();
                    })
                .ConfigureAppConfiguration(config => {
                    // enable settings file
                    config.AddJsonFile("appsettings.json");
                    config.AddEnvironmentVariables();
                    // enable user screts
                    config.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
                    config.Build();
                })
                .Build();
            host.Run();
        }
    }
}