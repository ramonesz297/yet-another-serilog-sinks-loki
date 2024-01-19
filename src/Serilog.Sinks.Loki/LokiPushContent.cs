using Serilog.Events;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Serilog.Sinks.Loki
{
    internal class LokiPushContent : HttpContent
    {
        private static readonly MediaTypeHeaderValue _defaultContentType = new MediaTypeHeaderValue("application/json");
        private readonly LokiMessageWriter _writer;
        private readonly IEnumerable<LogEvent> _events;
        private readonly PooledTextWriterAndByteBufferWriterOwner _bufferOwner;
        private LokiPushContent(LokiMessageWriter writer, IEnumerable<LogEvent> events)
        {
            _writer = writer;
            _events = events;
            Headers.ContentType = _defaultContentType;
            _bufferOwner = PooledTextWriterAndByteBufferWriterOwner.Instance;
        }

        internal static HttpContent Create(LokiMessageWriter writer, IEnumerable<LogEvent> events) => new LokiPushContent(writer, events);

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            var p = _bufferOwner.RentBufferWriter();

            using var writer = new Utf8JsonWriter(p);

            _writer.Write(writer, _events);

            writer.Flush();

            await stream.WriteAsync(p.WrittenMemory).ConfigureAwait(false);

            _bufferOwner.Return(p);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}
