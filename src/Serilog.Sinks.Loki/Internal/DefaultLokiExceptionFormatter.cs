// This file is part of the project licensed under the MIT License.
// See the LICENSE file in the project root for more information.


using Serilog.Events;
using System.Text.Json;

namespace Serilog.Sinks.Loki.Internal
{
    internal sealed class DefaultLokiExceptionFormatter : ILokiExceptionFormatter
    {
        private static readonly JsonEncodedText _type = JsonEncodedText.Encode("Exception__Type");
        private static readonly JsonEncodedText _message = JsonEncodedText.Encode("Exception__Message");
        private static readonly JsonEncodedText _source = JsonEncodedText.Encode("Exception__Source");
        private static readonly JsonEncodedText _stackTrace = JsonEncodedText.Encode("Exception__StackTrace");
        private static readonly JsonEncodedText _innerException = JsonEncodedText.Encode("Exception__InnerException");

        public void Format(Utf8JsonWriter writer, Exception exception)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            writer.WriteString(_type, exception.GetType().FullName ?? "<null>");

            writer.WriteString(_message, exception.Message ?? "<null>");

            if (exception.Source is not null)
            {
                writer.WriteString(_source, exception.Source ?? "<null>");
            }

            writer.WriteString(_stackTrace, exception.StackTrace ?? "<null>");

            if (exception.InnerException is not null)
            {
                writer.WritePropertyName(_innerException);

                using var bf = new PooledByteBufferWriter();
                using var tw = new Utf8JsonWriter(bf);
                tw.WriteStartObject();
                Format(tw, exception.InnerException);
                tw.WriteEndObject();
                tw.Flush();

                writer.WriteStringValue(bf.WrittenMemory.Span);

            }
        }
    }
}
