using System.Diagnostics;
using System.Net.Http;

namespace Serilog.Sinks.Loki.Benchmark
{
    public static class WebAppHostHelpers
    {
        public static async Task WarmUpApplication(string uri)
        {
            var client = new HttpClient()
            {
                BaseAddress = new Uri(uri)
            };

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    var result = await client.GetAsync("/health");
                    Debug.Assert(result.IsSuccessStatusCode);
                }
                catch (Exception)
                {
                }
            }

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    var result = await client.PostAsync("loki/api/v1/push", new StringContent("test content"));
                    Debug.Assert(result.IsSuccessStatusCode);
                }
                catch (Exception)
                {
                }
            }
        }
    }


}
