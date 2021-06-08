using HttpTriggerSample.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
                .Build();

            host.Run();
        }
    }
}