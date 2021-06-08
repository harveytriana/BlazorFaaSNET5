using HttpTriggerSample.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;

namespace HttpTriggerSample
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(
                    services => {
                        services.AddHttpClient();
                        services.AddSingleton<CurrencyTools>();
                    })
                .ConfigureAppConfiguration(x => {
                    x.AddJsonFile("appsettings.json");
                    x.AddEnvironmentVariables();
                    x.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
                    x.Build();
                })
                .Build();
            host.Run();
        }
    }
}