using HttpTriggerSample.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;

namespace HttpTriggerSample
{
    public class Test
    {
        readonly CurrencyLayerSettings _settings;

        public Test(IConfiguration configuration)
        {
            _settings = configuration.GetSection("CurrencyLayer").Get<CurrencyLayerSettings>();
        }

        [Function("Test")]
        public string Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            return $"Setting: {_settings.BaseUrl}";
        }
    }
}
