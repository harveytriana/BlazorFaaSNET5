using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using HttpTriggerSample.Models;
using HttpTriggerSample.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HttpTriggerSample
{
    public class DollarPrice
    {
        const string
            ACCESS_KEY = "398166e843713aa66aef35b0e58347e4",
            BASE_URL = "http://api.currencylayer.com/",
            ENDPOINT = "live";

        readonly HttpClient _httpClient;
        readonly CurrencyTools _currencyTools;
        readonly CurrencyLayerSettings _settings;
        
        public DollarPrice(
            IHttpClientFactory clientFactory,
            IConfiguration configuration,
            CurrencyTools currencyTools)
        {
            _httpClient = clientFactory.CreateClient();
            _currencyTools = currencyTools;
            _settings = configuration.GetSection("CurrencyLayer").Get<CurrencyLayerSettings>();
        }

        // Sample: http://localhost:7071/api/DollarPrice?currency=EUR

        [Function("DollarPrice")]
        public async Task<DollarPriceResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req,
            string currency,
            FunctionContext executionContext)
        {
            if (string.IsNullOrEmpty(currency)) {
                goto finish;
            }

            var url = $"{BASE_URL}/{ENDPOINT}?access_key={ACCESS_KEY}&source=USD&currencies={currency}";
            try {
                var data = await _httpClient.GetFromJsonAsync<CurrencyLayerResult>(url);
                return new DollarPriceResult {
                    Currency = data.Quotes.First().Key,
                    Price = data.Quotes.First().Value,
                    CurrencySymbol = _currencyTools.GetCurrencySymbol(currency),
                    TimeStamp = _currencyTools.UnixTimeStampToDateTime(data.Timestamp)
                };
            }
            catch (Exception e) {
                executionContext.GetLogger("DollarPrice").LogError($"Exception: {e.Message}");
            }
finish:
            return null;
        }
    }
}
