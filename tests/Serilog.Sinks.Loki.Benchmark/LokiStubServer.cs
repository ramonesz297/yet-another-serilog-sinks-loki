// This file is part of the project licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using System.Net;
using System.Net.Sockets;

namespace Serilog.Sinks.Loki.Benchmark
{
    internal sealed class LokiStubServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _serverTask;

        private long _requestCount;
        private long _requestBodyBytes;

        private LokiStubServer(string prefix)
        {
            BaseUri = new Uri(prefix);

            _listener = new HttpListener();
            _listener.Prefixes.Add(prefix);
            _listener.Start();

            _serverTask = Task.Run(() => RunAsync(_cts.Token));
        }

        public Uri BaseUri { get; }

        public long RequestCount => Interlocked.Read(ref _requestCount);

        public long RequestBodyBytes => Interlocked.Read(ref _requestBodyBytes);

        public static LokiStubServer Start()
        {
            var port = GetFreePort();
            return new LokiStubServer($"http://127.0.0.1:{port}/");
        }

        public void ResetMetrics()
        {
            Interlocked.Exchange(ref _requestCount, 0);
            Interlocked.Exchange(ref _requestBodyBytes, 0);
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                HttpListenerContext? context = null;

                try
                {
                    context = await _listener.GetContextAsync().ConfigureAwait(false);
                }
                catch (HttpListenerException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                _ = Task.Run(() => HandleAsync(context, cancellationToken), cancellationToken);
            }
        }

        private async Task HandleAsync(HttpListenerContext context, CancellationToken cancellationToken)
        {
            try
            {
                if (context.Request.HasEntityBody)
                {
                    await context.Request.InputStream.CopyToAsync(Stream.Null, cancellationToken).ConfigureAwait(false);

                    if (context.Request.ContentLength64 > 0)
                    {
                        Interlocked.Add(ref _requestBodyBytes, context.Request.ContentLength64);
                    }
                }

                Interlocked.Increment(ref _requestCount);

                context.Response.StatusCode = (int)HttpStatusCode.NoContent;
                context.Response.ContentLength64 = 0;
                context.Response.Close();
            }
            catch
            {
                try
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentLength64 = 0;
                    context.Response.Close();
                }
                catch
                {
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();

            try
            {
                _listener.Stop();
                _listener.Close();
            }
            catch
            {
            }

            try
            {
                _serverTask.GetAwaiter().GetResult();
            }
            catch
            {
            }

            _cts.Dispose();
        }

        private static int GetFreePort()
        {
            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
    }
}