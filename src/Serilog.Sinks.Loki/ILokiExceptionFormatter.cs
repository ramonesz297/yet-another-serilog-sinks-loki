using System.Text.Json;

namespace Serilog.Sinks.Loki
{
    /// <summary>
    /// Represents a formatter for exceptions in the Loki sink.
    /// </summary>
    public interface ILokiExceptionFormatter
    {
        /// <summary>
        /// Formats the exception to the provided <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The writer to format the exception to.</param>
        /// <param name="exception">The exception to format.</param>
        void Format(Utf8JsonWriter writer, Exception exception);
    }
}
