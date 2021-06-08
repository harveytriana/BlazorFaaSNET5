using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorFaaS
{
    public class Program
    {
        const string DEVELOPMENT_STORAGE = "http://localhost:7071/";

        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri(DEVELOPMENT_STORAGE)
            });

            await builder.Build().RunAsync();
        }
    }
}
