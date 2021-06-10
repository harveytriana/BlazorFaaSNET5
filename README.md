# Blazor and FaaS (Part 1)

*Serverless functions in isolated model and their consumption from Blazor WASM.*

The strategic and economic benefits of serverless computing are remarkable. There is extensive documentation on this elegant paradigm. This article focuses on code, and as it relates to C# programmers. Although the official documentation puts us on the right path, there are subtleties that are not easy to solve. On the other hand, when it comes to creating a Blazor application that consumes serverless functions, you will find important points that you should keep in mind.

This article focuses on the *out-of-process* or *isolated* model, which appears with NET5. This means that the serverless application is no longer loaded onto the host, but runs on its own and communicates with the function runtime.

> *Note. When I refer to the word **function** it should read as Azure Function.*

In the first part of this article, I am going to go into detail with the type of function that is classified in *HTTP Trigger*. The following topics are covered here.

- How to write a function that takes one input object and returns another.
- How to write a function that consumes a REST API and acts as a microservice.

For this last point:

- How to create an HTTP service within the function by dependency injection.
- How to use configuration data in a function.
- How to use a personalized service within a function.
- How to use User Secrets in a function.
- How to consume a REST API from the function and return an object in the response.
- How to resolve CORS issue for serverless functions in development environment.

From the Blazor point of view we figure out how to consume the described functions.

#### Requirements

The IDE is ideal for this type of solution, however you can also work with *vscode*. For the IDE we only need,

- Visual Studio 2019, version 16.x, with NET5
- The Azure SDK installed

#### Example Solution

The solution consists of two projects: (1) An Azure Functions application, and (2) a non-hosted Blazor WebAssembly application.

## Azure functions application

It concerns a template project **Azure Funtions**, with solution name: **BlazorFaaS**, and project name **HttpTriggerSample**. Then I select **NET (isolate)** and **Http trigger** as schema, **Storage Emulator** and finally **Anonymous** as authorization level. We execute to verify that everything is going well, and the Emulator starts naturally. System permissions are often requested.

### The Hypotenuse Function

Initially I write a function in which an object is sent as a parameter, and a value is returned. Although the function is very simple, it illustrates the general case for this type of schema. I use the *Hypotenuse* function as an example, which takes two numbers as parameters, the legs, and returns a value. In mathematical terms:

![](https://github.com/harveytriana/BlazorFaaSNET5/blob/master/Screenshypotenuse.png)

To address this simple problem you could just use a GET and pass the legs in the URL. However, the goal is to illustrate how to pass an object in a POST and expect a value (or type). For this I create a type `Legs` for the parameter; I use a record type, which has advantages such as less code and more efficiency, in addition, the input values are not modified, a mutable object is not needed.

In the project, *Add New Azure Function*,  with name *Hypotenuse* of type *Http trigger*. We replace all the code with the following.

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace HttpTriggerSample
{
    public static class Hypotenuse
    {
        [Function("Hypotenuse")]
        public static async Task<double> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
            HttpRequestData req,
            FunctionContext executionContext)
        {
            try {
                var legs = await req.ReadFromJsonAsync<Legs>();
                return Math.Sqrt(Math.Pow(legs.X, 2.0) + Math.Pow(legs.Y, 2.0));
            }
            catch (Exception e) {
                executionContext.GetLogger("Hypotenuse")
							    .LogError($"Exception: {e.Message}");
            }
            return -999.25;
        }
    }

	// input object
    record Legs(double X, double Y);
}
```

Some details,

- The object passed in the request body is encoded inside `req`, and decoded in a single line with `req.ReadFromJsonAsync<T>`.
- The log is created only if necessary, that is, in the case of an exception.
- The return of -999.25 on error is arbitrary. In engineering we sometimes use this type of protocol value to differentiate from zero.

We can test the function when executing the project. For example, from Postman we use the URL http://localhost:7071/api/Hypotenuse, POST, with body: `{" X ": 2," Y ": 3}`,  to get the result: `3.605551275463989`.

### The Dollar Price Function

This second function is more complex. It involves using a third-party REST API to obtain the current dollar price from an ISO currency identifier.

The API I use here is https://currencylayer.com/, of which we can make a free subscription for development tests. In this way we obtain the ACCESS_KEY, which we will later add as a *user secret*. This API returns an object with structure:

```json
{
	"success":true,
	"timestamp": 1623164188,
	"source": USD,
	"quotes": {
		"USDCAD": 1.211891,
		"USDEUR": 0.830538,
		"USDGBP": 0.726740,
	}
}
```

From this answer we map in C# with the following type:

```csharp
public record CurrencyLayerResult(
	bool Success,
	long Timestamp,
	Dictionary<string, decimal> Quotes
);
```

> As I mentioned earlier, using record instead of class is good practice for reading an API, and the serialization is the same. See as an optimization.

To implement this function I use three types. One for the API login settings, another for the response decoding, and finally one for the function response. I have arranged them in a single file named `DataLayer`,  in a `Models` folder, with the following code:

```csharp
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
```

The `DollarPrice` function has a second objective and it is to show how we implement the postulate themes, that is, Enable use of dependency injection, custom settings configuration, and user secrets.

In the NET5 version we have the `Program` class, where we can configure the execution behavior of the application.

```csharp
using HttpTriggerSample.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace HttpTriggerSample
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                // enable DI
                .ConfigureServices(
                    services => {
                        // enable http service
                        services.AddHttpClient();
                        // enable custom service
                        services.AddSingleton<CurrencyTools>();
                    })
                // enable settings file
                .ConfigureAppConfiguration(config => {
                    config.AddJsonFile("appsettings.json");
                    config.AddEnvironmentVariables();
                    // enable user screts
                    config.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
                    config.Build();
                })
                .Build();
            host.Run();
        }
    }
}
```

We add a file named *appsettings.json*,  with attribute *Copy if new* , with the following information:

```json
{
	"CurrencyLayer": {
		"BaseUrl": "http://api.currencylayer.com/",
		"EndPoint": "live"
	}
}
```

The file only has two pieces of data from the structure that you create for the connection to the API (`CurrencyLayerSettings`). I add the access key in a user secret. For this we need the *Microsoft.Extensions.Configuration.UserSecrets* reference. Then, by console or by the IDE, we add *User Secrets* with the following information:

```json
{
    "CurrencyLayer:AccessKey": "YOUR_API_KEY"
}
```

**Application Services**

In the response of the function I want to add the currency symbol, and convert the UNIX date to C# date. For this I have added a custom `CurrencyTools` service, in a `Services` folder.

```csharp
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
```

This completes what is required for the `DolarPrice` function.

### The function

In the project, *Add New Azure Function*,  with name *DollarPrice* of type *Http trigger*. We replace all the code with the following.

```csharp
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
* API SOURCE:
* https://currencylayer.com/
* Sample:
* http://localhost:7071/api/DollarPrice?currency=EUR
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
            IConfiguration config,
            CurrencyTools currencyTools)
        {
            _httpClient = clientFactory.CreateClient();
            _currencyTools = currencyTools;
            _settings = config.GetSection("CurrencyLayer").Get<CurrencyLayerSettings>();
        }

        [Function("DollarPrice")]
        public async Task<DollarPriceResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
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
```

As it reads, we use dependency injection as is normally done in NET Core.

If you want this function can also be tested with Postman or with the simple URL from a browser. In project execution, use the following URL: http://localhost:7071/api/DollarPrice?Currency=EUR, to get something like:

```json
{
	"TimeStamp": "2021-06-08T02:14:03Z",
	"Currency": "USDEUR",
	"CurrencySymbol": "€",
	"Price": 0.820501
}
```

## The Blazor App

It is a non-hosted Blazor WebAssembly project named *BlazorSaaS*. We need to enable CORS in the functions project, for which we make an additional adjustment in the functions project and it consists of adding CORS in *local.settings.json*,  with the following information:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  },
  "Host": {
    "CORS": "*"
  }
}
```

We configure the Blazor application so that the HTTP service points to the address of the functions or the publications in Azure, depending on the execution environment, that is, Azure or local storage.

```csharp
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
```

The static variable `IS_DEVELOPMENT` simplifies the code in the Blazor components where we would have to inject `IWebAssemblyHostEnvironment`. This variable is suitable if, for example, the functions run in Azure and we need to add the key to the URL, in case they are not anonymous.

The Blazor *HypotenusePage* component

As the following code reads, it is normal Blazor code, where an API is invoked over HTTP. I did not publish the HTML here, it is already simple, and a designer will write better.

![](https://github.com/harveytriana/BlazorFaaSNET5/blob/master/Screensbz_faas-1.png)

*HypotenusePage*

```csharp
...
@code {
	string result;
	double _x, _y, _h;

	record Legs(double X, double Y);

	protected override async Task OnInitializedAsync()
	{
		_x = 3;
		_y = 4;
		await CalculateHypotenuse();
	}

	protected async Task CalculateHypotenuse()
	{
		try
		{
			var legs = new Legs(_x, _y);
			// call the api
			var response = await _httpClient.PostAsJsonAsync<Legs>("api/Hypotenuse", legs);
			var json = await response.Content.ReadAsStringAsync();
			var so = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};
			_h = JsonSerializer.Deserialize<double>(json, so);

			result = $"Hypotenuse({_x:0.00}, {_y:0.00}) = {_h:0.00}";
		}
		catch (Exception e)
		{
			result = $"Exception: {e.Message}";
		}
	}
}
```

### The Blazor *DollarPricePage* component

Likewise, it is a Blazor component that consumes an API. To give an elegant UI, I added the ISO three letter currency list next to the country name, for user selection. This data can be downloaded from the API vendor, [»»](https://currencylayer.com/site_downloads/cl-currencies-select-option.txt)

![](https://github.com/harveytriana/BlazorFaaSNET5/blob/master/Screensbz_faas-2.png)

*Componente DollarPricePage*

```csharp
@using System.Text.Json
@using Client.Utils;
@page "/fn-dollar-price"
@inject HttpClient _httpClient

<h2>Dollar Price Azure Function</h2>
<hr />
<p>This component demonstrates a Http Trigger</p>
<br />
@if (ls != null)
{
	<h4>Select Currency</h4>
	<select class="form-control" @onchange="ChangeCurrency">
		@foreach (var i in ls.Keys)
		{
		 <option value="@i">@i @ls[i]</option>
		}
	</select>
	<br>
	<hr>
	<h4>Dollar Price for @currency</h4>
	<div class="result">
		@currencySymbol	@dollarPrice
	</div>
	<p class="prompt">@prompt</p>
}

@code {
	record DollarPrice(DateTime TimeStamp, string Currency, string CurrencySymbol, decimal Price);

	IDictionary<string, string> ls;

	string currency;
	string currencyText;
	string currencySymbol;
	bool busy;
	string dollarPrice;
	string prompt;

	protected override async Task OnInitializedAsync()
	{
		var js = ResourceReader.Read("cl-currencies.json");

		ls = JsonSerializer.Deserialize<IDictionary<string, string>>(js);

		// initilize value
		await Task.Delay(300);
		currency = "EUR";
		await GetDollarPrice();
	}

	async Task ChangeCurrency(ChangeEventArgs e)
	{
		var y = e.Value.ToString();
		if (currency != y)
		{
			currency = y;
			currencyText = ls[currency];
			await GetDollarPrice();
		}
	}

	async Task GetDollarPrice()
	{
		if (busy)
		{
			return;
		}
		busy = true;
		try
		{
			var functionUrl = $"api/DollarPrice?currency={currency}";
			var data = await _httpClient.GetFromJsonAsync<DollarPrice>(functionUrl);
			if (data != null) {// update UI
				dollarPrice = data.Price.ToString("#,##0.00");
				currencySymbol = data.CurrencySymbol;
				prompt = $"Time Stamp: {data.TimeStamp}";
			}
			else {
				prompt = $"Unexpectedly returns null";
			}
		}
		catch (Exception e)
		{
			prompt = $"Exception: {e.Message}";
		}
		busy = false;
	}
}
```

### Considerations in Azure

After publishing the function app to Azure, the configuration settings for user secrets are resolved by creating a new *Application Setting* with name: *CurrencyLayer: AccessKey*,  and value: *YOUR_API_KEY*. In this way Azure maps the user secret well, and put it in the `CurrencyLayerSettings` class. *I should be mentioned, that the creation of a key-vault is not required*

Similarly, we must enable CORS in Azure so that the resource is allowed to be used from the Blazor application. For this we go to the CORS section of the resource in Azure, and we allow all origins with an asterisk.

![](https://github.com/harveytriana/BlazorFaaSNET5/blob/master/Screensaz-cors.png)

Now you can run the Blazor app to check that everything is going great.

---

This article belongs to the Blog »» [BlazorSpread.net](https://www.blazorspread.net) 

---

*References*

- [Azure Functions documentation](https://docs.microsoft.com/en-us/azure/azure-functions/)
- Blog [Oscar van Tol](https://dev.to/oscarvantol)

`MIT license. Author: Harvey Triana. Contact: admin @ blazorspread.net`

---

*Last edition: 06-09-2021*
