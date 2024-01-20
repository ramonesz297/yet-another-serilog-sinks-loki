using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.Loki
{
    internal class LokiSink : IBatchedLogEventSink, IDisposable
    {
        private readonly LokiSinkConfigurations _configurations;
        private readonly LokiLogEventComparer _comparer;
        private readonly LokiMessageWriter _lokiMessageWriter;
        private readonly HttpClient _httpClient;
        private readonly Uri _requestUri;
        private readonly PooledTextWriterAndByteBufferWriterOwner _bufferWriterOwner;
        internal LokiSink(LokiSinkConfigurations configurations, HttpClient httpClient)
        {
            _configurations = configurations;
            _comparer = new LokiLogEventComparer(_configurations);
            _bufferWriterOwner = new PooledTextWriterAndByteBufferWriterOwner();
            _lokiMessageWriter = new LokiMessageWriter(_configurations, _bufferWriterOwner, _comparer);
            _httpClient = httpClient;
            _httpClient.BaseAddress = configurations.Url;
            _httpClient.SetCredentials(configurations.Credentials);
            _httpClient.SetTenant(configurations.Tenant);
            _requestUri = new Uri(configurations.Url, "/loki/api/v1/push");
        }


        public Task EmitBatchAsync(IEnumerable<LogEvent> batch)
        {
            var content = LokiPushContent.Create(_lokiMessageWriter, _bufferWriterOwner, batch);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _requestUri)
            {
                Content = content,
            };

            return _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
        }

        public Task OnEmptyBatchAsync()
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            _bufferWriterOwner.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
