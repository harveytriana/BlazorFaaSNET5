// ======================================
// BlazorSpread.net
// ======================================
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorFaaS
{
    public class Program
    {
        public static bool IS_DEVELOPMENT { get; private set; }

        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            IS_DEVELOPMENT = builder.HostEnvironment.IsDevelopment();

            var functionsbase = IS_DEVELOPMENT ?
                "http://localhost:7071" :
                "https://YOUR_FUNCTION_APP.azurewebsites.net";

            builder.Services.AddScoped(sp => new HttpClient {
                BaseAddress = new Uri(functionsbase)
            });

            await builder.Build().RunAsync();
        }
    }
}
