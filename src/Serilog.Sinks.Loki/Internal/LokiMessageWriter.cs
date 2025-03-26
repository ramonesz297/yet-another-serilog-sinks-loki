// This file is part of the project licensed under the MIT License.
// See the LICENSE file in the project root for more information.


using Serilog.Events;
using System.Buffers.Text;
using System.Text.Json;

namespace Serilog.Sinks.Loki.Internal
{
    internal class LokiMessageWriter
    {
        private const string _levelLiteral = "level";
        private static readonly JsonEncodedText _stream = JsonEncodedText.Encode("stream");
        private static readonly JsonEncodedText _streams = JsonEncodedText.Encode("streams");
        private static readonly JsonEncodedText _level = JsonEncodedText.Encode(_levelLiteral);
        private static readonly JsonEncodedText _values = JsonEncodedText.Encode("values");
        private static readonly JsonEncodedText _message = JsonEncodedText.Encode("Message");
        private static readonly JsonEncodedText _messageTemplate = JsonEncodedText.Encode("MessageTemplate");
        private static readonly JsonEncodedText _exception = JsonEncodedText.Encode("Exception");
        private static readonly JsonEncodedText _traceId = JsonEncodedText.Encode("TraceId");
        private static readonly JsonEncodedText _spanId = JsonEncodedText.Encode("SpanId");
        private static readonly JsonEncodedText _trace = JsonEncodedText.Encode("trace");
        private static readonly JsonEncodedText _debug = JsonEncodedText.Encode("debug");
        private static readonly JsonEncodedText _info = JsonEncodedText.Encode("info");
        private static readonly JsonEncodedText _warning = JsonEncodedText.Encode("warning");
        private static readonly JsonEncodedText _error = JsonEncodedText.Encode("error");
        private static readonly JsonEncodedText _critical = JsonEncodedText.Encode("critical");
        private static readonly JsonEncodedText _unknown = JsonEncodedText.Encode("unknown");
        private static readonly JsonEncodedText _type = JsonEncodedText.Encode("type");

        private readonly LokiSinkConfigurations _configurations;
        private readonly LokiLogEventComparer _comparer;
        private readonly ILokiExceptionFormatter _exceptionFormatter;
        internal LokiMessageWriter(LokiSinkConfigurations configurations, LokiLogEventComparer comparer, ILokiExceptionFormatter exceptionFormatter)
        {
            _configurations = configurations;
            _comparer = comparer;
            _exceptionFormatter = exceptionFormatter;
        }

        private static JsonEncodedText MapLogLevel(LogEventLevel logLevel)
        {
            return logLevel switch
            {
                LogEventLevel.Verbose => _trace,
                LogEventLevel.Debug => _debug,
                LogEventLevel.Information => _info,
                LogEventLevel.Warning => _warning,
                LogEventLevel.Error => _error,
                LogEventLevel.Fatal => _critical,
                _ => _unknown
            };
        }

        public void Write(Utf8JsonWriter writer, IReadOnlyCollection<LogEvent> events)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(_streams);
            writer.WriteStartArray();

            foreach (var stream in events.GroupBy(x => x, _comparer))
            {
                WriteStream(writer, stream.Key, stream);
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        private void WriteLabels(Utf8JsonWriter writer, LogEvent head)
        {
            writer.WritePropertyName(_stream);
            writer.WriteStartObject();

            if (_configurations.HandleLogLevelAsLabel)
            {
                writer.WritePropertyName(_level);
                writer.WriteStringValue(MapLogLevel(head.Level));
            }

            for (int i = 0; i < _configurations.Labels.Length; i++)
            {
                var label = _configurations.Labels[i];

                if (label.Key == _levelLiteral && _configurations.HandleLogLevelAsLabel)
                {
                    continue;
                }

                writer.WriteString(label.Key, label.Value);
            }

            for (int i = 0; i < _configurations.PropertiesAsLabels.Length; i++)
            {
                var label = _configurations.PropertiesAsLabels[i];

                if (head.Properties.TryGetValue(label, out var value))
                {
                    if (value is ScalarValue scalarValue && scalarValue.Value != null)
                    {
                        //if user want to expose log level as label and there are property with same name
                        //then we need to rename property name
                        if (label == _levelLiteral && _configurations.HandleLogLevelAsLabel)
                        {
                            writer.WritePropertyName(_level);
                        }
                        //if user already declared the same property as lable in global labels and in properties as labels
                        //then we need to rename property name
                        else if (Array.Exists(_configurations.Labels, x => x.Key == label))
                        {
                            writer.WritePropertyName($"_{label}");
                        }
                        else
                        {
                            writer.WritePropertyName(label);
                        }

                        if (!scalarValue.WriteAsStringValue(writer))
                        {
                            writer.WriteStringValue("");
                        }
                    }
                }
            }

            writer.WriteEndObject();
        }

        private static void WriteLogMessageStringValue(Utf8JsonWriter writer, LogEvent logEvent)
        {
            using var bf = new PooledByteBufferWriter();
            using var tw = new Utf8TextWriter(bf);
            logEvent.RenderMessage(tw);
            tw.Flush();
            writer.WriteStringValue(tw.WrittenMemory.Span);
        }

        private static void WriteStructuredValue(string propertyName, Utf8JsonWriter writer, StructureValue structureValue)
        {
            foreach (var item in structureValue.Properties)
            {
                var newPropertyName = $"{propertyName}__{item.Name}";
                WriteProperty(newPropertyName, writer, item.Value);
            }
        }

        private static void WriteSequenceValue(string propertyName, Utf8JsonWriter writer, SequenceValue sequence)
        {
            var i = 0;
            foreach (var item in sequence.Elements)
            {
                var newPropertyName = $"{propertyName}__{i++}";
                WriteProperty(newPropertyName, writer, item);
            }
        }

        private static void WriteDictionaryValue(string propertyName, Utf8JsonWriter writer, DictionaryValue value)
        {
            if (value.Elements is null)
            {
                writer.WritePropertyName(propertyName);
                writer.WriteStringValue("<null>");
                return;
            }

            foreach (var item in value.Elements)
            {
                var key = $"{propertyName}__{item.Key.Value!.ToString()}";
                WriteProperty(key, writer, item.Value);
            }
        }

        private static void WriteProperty(string propertyName, Utf8JsonWriter writer, LogEventPropertyValue? value)
        {
            switch (value)
            {
                case null:
                    writer.WritePropertyName(propertyName);
                    writer.WriteStringValue("<null>");
                    break;
                case ScalarValue scalarValue:
                    writer.WritePropertyName(propertyName);
                    scalarValue.WriteAsNonNullableStringValue(writer);
                    break;
                case DictionaryValue dictionaryValue:
                    WriteDictionaryValue(propertyName, writer, dictionaryValue);
                    break;
                case SequenceValue sequenceValue:
                    WriteSequenceValue(propertyName, writer, sequenceValue);
                    break;
                case StructureValue structureValue:
                    WriteStructuredValue(propertyName, writer, structureValue);
                    break;
                default:
                    writer.WritePropertyName(propertyName);
                    writer.WriteStringValue(value?.ToString() ?? "<null>");
                    break;
            }
        }

        private void WriteLogMessageAsJson(Utf8JsonWriter destination, LogEvent logEvent)
        {
            WriteLogMessageStringValue(destination, logEvent);

            destination.WriteStartObject();

            if (logEvent.Exception is not null)
            {
                _exceptionFormatter.Format(destination, logEvent.Exception);
            }

            if (_configurations.EnrichTraceId && logEvent.TraceId.HasValue)
            {
                destination.WriteString(_traceId, logEvent.TraceId.Value.ToString());
            }

            if (_configurations.EnrichSpanId && logEvent.SpanId.HasValue)
            {
                destination.WriteString(_spanId, logEvent.SpanId.Value.ToString());
            }

            foreach (var item in logEvent.Properties)
            {
                WriteProperty(item.Key, destination, item.Value);
            }

            destination.WriteEndObject();

            destination.Flush();
        }



        private void WriteLogs(Utf8JsonWriter writer, IEnumerable<LogEvent> events)
        {
            writer.WritePropertyName(_values);

            writer.WriteStartArray();

            Span<byte> buffer = stackalloc byte[25];

            foreach (var logEvent in events)
            {
                writer.WriteStartArray();

                var timestamp = logEvent.Timestamp.ToUnixNanoseconds();

                Utf8Formatter.TryFormat(timestamp, buffer, out var charsWritten);

                writer.WriteStringValue(buffer.Slice(0, charsWritten));

                WriteLogMessageAsJson(writer, logEvent);

                writer.WriteEndArray();

                buffer.Clear();
            }

            writer.WriteEndArray();
        }

        private void WriteStream(Utf8JsonWriter writer, LogEvent head, IEnumerable<LogEvent> events)
        {
            writer.WriteStartObject();

            WriteLabels(writer, head);

            WriteLogs(writer, events);

            writer.WriteEndObject();
        }
    }
}
