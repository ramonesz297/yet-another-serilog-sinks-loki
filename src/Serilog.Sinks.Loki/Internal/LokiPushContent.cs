// This file is part of the project licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using Serilog.Events;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Serilog.Sinks.Loki.Internal
{
    internal sealed class LokiPushContent : HttpContent
    {
        private static readonly MediaTypeHeaderValue _defaultContentType = new("application/json");
        private readonly LokiMessageWriter _writer;
        private readonly IReadOnlyCollection<LogEvent> _events;
        private LokiPushContent(LokiMessageWriter writer, IReadOnlyCollection<LogEvent> events)
        {
            _writer = writer;
            _events = events;
            Headers.ContentType = _defaultContentType;
        }

        internal static HttpContent Create(LokiMessageWriter writer,
                                           IReadOnlyCollection<LogEvent> events) => new LokiPushContent(writer, events);

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            using var bufferWriter = new PooledByteBufferWriter(1024 * 4);
            await using var writer = new Utf8JsonWriter(bufferWriter);

            _writer.Write(writer, _events);

            writer.Flush();
#if NETCOREAPP
            await stream.WriteAsync(bufferWriter.WrittenMemory);
#else
            await stream.WriteAsync(bufferWriter.Buffer, 0, bufferWriter.WrittenCount);
#endif
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}
