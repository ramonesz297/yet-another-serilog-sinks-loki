using System.Text.Json;

namespace Serilog.Sinks.Loki.Internal
{
    internal sealed class DefaultLokiExceptionFormatter : ILokiExceptionFormatter
    {
        public void Format(Utf8JsonWriter writer, Exception exception)
        {
            ArgumentNullException.ThrowIfNull(writer, nameof(writer));
            ArgumentNullException.ThrowIfNull(exception, nameof(exception));

            writer.WriteStartObject();

            writer.WriteString("Type"u8, exception.GetType().FullName);

            writer.WriteString("Message"u8, exception.Message);

            if (exception.Source is not null)
            {
                writer.WriteString("Source"u8, exception.Source);
            }

            writer.WriteString("StackTrace"u8, exception.StackTrace);

            if (exception.InnerException is not null)
            {
                writer.WritePropertyName("InnerException"u8);

                Format(writer, exception.InnerException);
            }

            writer.WriteEndObject();
        }
    }
}
