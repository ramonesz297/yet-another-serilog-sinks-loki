// This file is part of the project licensed under the MIT License.
// See the LICENSE file in the project root for more information.


using System.Text.Json;

namespace Serilog.Sinks.Loki.Internal
{
    internal sealed class DefaultLokiExceptionFormatter : ILokiExceptionFormatter
    {
        private static readonly JsonEncodedText _type = JsonEncodedText.Encode("Type");
        private static readonly JsonEncodedText _message = JsonEncodedText.Encode("Message");
        private static readonly JsonEncodedText _source = JsonEncodedText.Encode("Source");
        private static readonly JsonEncodedText _stackTrace = JsonEncodedText.Encode("StackTrace");
        private static readonly JsonEncodedText _innerException = JsonEncodedText.Encode("InnerException");

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

            writer.WriteStartObject();

            writer.WriteString(_type, exception.GetType().FullName);

            writer.WriteString(_message, exception.Message);

            if (exception.Source is not null)
            {
                writer.WriteString(_source, exception.Source);
            }

            writer.WriteString(_stackTrace, exception.StackTrace);

            if (exception.InnerException is not null)
            {
                writer.WritePropertyName(_innerException);

                Format(writer, exception.InnerException);
            }

            writer.WriteEndObject();
        }
    }
}
