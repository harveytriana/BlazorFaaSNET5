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
                // enables DI
                .ConfigureServices(
                    services => {
                        services.AddHttpClient();
                        services.AddSingleton<CurrencyTools>();
                    })
                // enables settings file
                .ConfigureAppConfiguration(config => {
                    config.AddJsonFile("appsettings.json");
                    config.AddEnvironmentVariables();
                    // enables user screts
                    config.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
                    config.Build();
                })
                .Build();
            host.Run();
        }
    }
}