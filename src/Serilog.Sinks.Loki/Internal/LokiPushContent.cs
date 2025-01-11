// This file is part of the project licensed under the MIT License.
// See the LICENSE file in the project root for more information.


using Serilog.Events;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Serilog.Sinks.Loki.Internal
{
    internal class LokiPushContent : HttpContent
    {
        private static readonly MediaTypeHeaderValue _defaultContentType = new MediaTypeHeaderValue("application/json");
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

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            using var bufferWriter = new PooledByteBufferWriter(1024 * 4);

            using var writer = new Utf8JsonWriter(bufferWriter);

            _writer.Write(writer, _events);

            writer.Flush();
#if NETCOREAPP
            return stream.WriteAsync(bufferWriter.WrittenMemory).AsTask();
#else
            return stream.WriteAsync(bufferWriter.Buffer, 0, bufferWriter.WrittenCount);
#endif
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}
