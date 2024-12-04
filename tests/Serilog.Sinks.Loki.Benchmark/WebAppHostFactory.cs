using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Diagnostics;
using System.Net;

namespace Serilog.Sinks.Loki.Benchmark
{
    public static class WebAppHostFactory
    {
        public static WebApplication Create(int port)
        {
            var builder = WebApplication.CreateSlimBuilder();
            builder.Logging.ClearProviders();

            builder.WebHost.ConfigureKestrel((context, serverOptions) =>
            {
                serverOptions.Listen(IPAddress.Loopback, port, o =>
                {
                    o.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                });
            });

            builder.Services.AddHealthChecks();

            var app = builder.Build();

            app.MapHealthChecks("/health").AllowAnonymous();

            app.MapPost("loki/api/v1/push", async (HttpContext context) =>
            {
                var reader = context.Request.BodyReader;
                await reader.CopyToAsync(Stream.Null);
                return Results.Ok();
            }).AllowAnonymous();

            return app;
        }

        public static async Task<HttpClient> RunAppAsync(WebApplication app)
        {
            var client = await StartApp(app);

            await WarmUpApplication(app);

            return client;
        }

        private static Task<HttpClient> StartApp(WebApplication app)
        {
            var tcs = new TaskCompletionSource<HttpClient>();

            app.Lifetime.ApplicationStarted.Register(() =>
            {
                var client = new HttpClient()
                {
                    DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
                };
                client.BaseAddress = new Uri(app.Urls.First());
                tcs.SetResult(client);
            });

            _ = app.RunAsync();

            return tcs.Task;
        }

        private static async Task<HttpClient> WarmUpApplication(WebApplication app)
        {
            var client = new HttpClient()
            {
                DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher,
                BaseAddress = new Uri(app.Urls.First())
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

            return client;
        }
    }


}
