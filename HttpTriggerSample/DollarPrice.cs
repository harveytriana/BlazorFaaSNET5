// ======================================
// BlazorSpread.net
// ======================================
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
/*
* API SOURCE
* https://currencylayer.com/
* Sample
* http://localhost:7071/api/DollarPrice?currency=COP
*/
namespace HttpTriggerSample
{
    public class DollarPrice
    {
        readonly HttpClient _httpClient;
        readonly CurrencyTools _currencyTools;
        readonly CurrencyLayerSettings _settings;
        
        public DollarPrice(
            IHttpClientFactory clientFactory,
            IConfiguration configuration,
            CurrencyTools currencyTools)
        {
            _httpClient = clientFactory.CreateClient();
            _settings = configuration.GetSection("CurrencyLayer").Get<CurrencyLayerSettings>();
            _currencyTools = currencyTools;
        }

        [Function("DollarPrice")]
        public async Task<DollarPriceResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req,
            string currency,
            FunctionContext executionContext)
        {
            if (string.IsNullOrEmpty(currency)) {
                goto finish;
            }

            var url = $"{_settings.BaseUrl}/{_settings.EndPoint}?access_key=" 
                    + $"{_settings.AccessKey}&source=USD&currencies={currency}";
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
