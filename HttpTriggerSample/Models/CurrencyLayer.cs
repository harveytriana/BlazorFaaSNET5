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
    public record CurrencyLayerSettings
    {
        public string BaseUrl { get; set; }
        public string EndPoint { get; set; }
        public string AccessKey { get; set; }
    }

    /// <summary>
    /// Map API response
    /// </summary>
    public record CurrencyLayerResult(
        bool Success,
        long Timestamp,
        Dictionary<string, decimal> Quotes
    );

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
