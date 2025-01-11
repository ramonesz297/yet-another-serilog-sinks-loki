// This file is part of the project licensed under the MIT License.
// See the LICENSE file in the project root for more information.


using Serilog.Core;
using Serilog.Events;
using System.Net.Http;

namespace Serilog.Sinks.Loki.Internal
{

    internal sealed class LokiSink : IBatchedLogEventSink, IDisposable
    {
        private readonly LokiSinkConfigurations _configurations;
        private readonly LokiLogEventComparer _comparer;
        private readonly LokiMessageWriter _lokiMessageWriter;
        private readonly HttpClient _httpClient;
        private readonly Uri _requestUri;

        internal LokiSink(LokiSinkConfigurations configurations, HttpClient httpClient, ILokiExceptionFormatter exceptionFormatter)
        {
            _configurations = configurations;
            _comparer = new LokiLogEventComparer(_configurations);
            _lokiMessageWriter = new LokiMessageWriter(_configurations,  _comparer, exceptionFormatter);
            _httpClient = httpClient;
            _httpClient.BaseAddress = configurations.Url;
            _httpClient.SetCredentials(configurations.Credentials);
            _httpClient.SetTenant(configurations.Tenant);
            _requestUri = new Uri(configurations.Url, "/loki/api/v1/push");
        }

        public Task EmitBatchAsync(IReadOnlyCollection<LogEvent> batch)
        {
            var content = LokiPushContent.Create(_lokiMessageWriter,  batch);

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
        }
    }
}
