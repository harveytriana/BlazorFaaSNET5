using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorFaaS
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            var functionsbase = builder.HostEnvironment.IsDevelopment()?
                "http://localhost:7071" :
                "https://httptriggersample2021.azurewebsites.net";

            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri(functionsbase)
            });

            await builder.Build().RunAsync();
        }
    }
}
