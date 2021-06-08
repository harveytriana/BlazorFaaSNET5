// ======================================
// BlazorSpread.net
// ======================================
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace HttpTriggerSample.Utils
{
    public class CurrencyTools
    {
        readonly IDictionary<string, string> _ls;

        public CurrencyTools()
        {
            // cache currency symbols key-value
            _ls = CultureInfo
                .GetCultures(CultureTypes.AllCultures)
                .Where(c => !c.IsNeutralCulture)
                .Select(culture => {
                    try {
                        return new RegionInfo(culture.Name);
                    }
                    catch {
                        return null;
                    }
                })
                .Where(ri => ri != null)
                .GroupBy(ri => ri.ISOCurrencySymbol)
                .ToDictionary(x => x.Key, x => x.First().CurrencySymbol);
        }

        public string GetCurrencySymbol(string currency)
        {
            if (_ls.ContainsKey(currency)) {
                return _ls[currency];
            }
            return "";
        }

        public DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return dt.AddSeconds(unixTimeStamp);
        }
    }
}
