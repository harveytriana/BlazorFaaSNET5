using HttpTriggerSample.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;

namespace HttpTriggerSample
{
    public class Test
    {
        readonly CurrencyLayerSettings _settings;
        readonly string _keyTest;

        public Test(IConfiguration configuration)
        {
            _settings = configuration.GetSection("CurrencyLayer").Get<CurrencyLayerSettings>();
            _keyTest = configuration.GetValue<string>("KeyTest");
        }

        [Function("Test")]
        public string Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            return $"BaseUrl: {_settings.BaseUrl} | AccessKey: {_settings.AccessKey} | KeyTest: {_keyTest}";
        }
    }
}
