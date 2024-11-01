using Serilog.Events;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Serilog.Sinks.Loki.Internal
{
    internal class LokiPushContent : HttpContent
    {
        private static readonly MediaTypeHeaderValue _defaultContentType = new MediaTypeHeaderValue("application/json");
        private readonly LokiMessageWriter _writer;
        private readonly IReadOnlyCollection<LogEvent> _events;
        private readonly PooledTextWriterAndByteBufferWriterOwner _bufferOwner;
        private LokiPushContent(LokiMessageWriter writer, PooledTextWriterAndByteBufferWriterOwner bufferOwner, IReadOnlyCollection<LogEvent> events)
        {
            _writer = writer;
            _events = events;
            Headers.ContentType = _defaultContentType;
            _bufferOwner = bufferOwner;
        }

        internal static HttpContent Create(LokiMessageWriter writer,
                                           PooledTextWriterAndByteBufferWriterOwner bufferOwner,
                                           IReadOnlyCollection<LogEvent> events) => new LokiPushContent(writer, bufferOwner, events);

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        {
            var bufferWriter = _bufferOwner.RentBufferWriter();

            using var writer = new Utf8JsonWriter(bufferWriter);

            _writer.Write(writer, _events);

            writer.Flush();

            stream.Write(bufferWriter.WrittenMemory.Span);

            _bufferOwner.Return(bufferWriter);

            return Task.CompletedTask;
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}
