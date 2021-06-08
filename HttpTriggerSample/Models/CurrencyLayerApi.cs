// ======================================
// BlazorSpread.net
// ======================================
using System;
using System.Collections.Generic;

namespace HttpTriggerSample.Models
{
    /// <summary>
    /// API Settings
    /// </summary>
    public class CurrencyLayerApi
    {
        public string BaseUrl { get; set; }
        public string EndPoint { get; set; }
        public string AccessKey { get; set; }
    }

    /// <summary>
    /// Map https://currencylayer.com/ response
    /// </summary>
    public class CurrencyLayerResult
    {
        public bool Success { get; set; }
        public long Timestamp { get; set; }
        public Dictionary<string, decimal> Quotes { get; set; }
    }

    /// <summary>
    /// Function response
    /// </summary>
    public class DollarPriceResult
    {
        public DateTime TimeStamp { get; set; }
        public string Currency { get; set; }
        public string CurrencySymbol { get; set; }
        public decimal Price { get; set; }
    }
}
