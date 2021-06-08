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
                executionContext.GetLogger("Hypotenuse").LogError($"Exception: {e.Message}");
            }
            return 0.0;
        }
    }
    record Legs(double X, double Y);
}
