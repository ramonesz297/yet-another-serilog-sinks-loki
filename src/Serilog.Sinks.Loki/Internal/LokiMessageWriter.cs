using Serilog.Events;
using System.Runtime.CompilerServices;
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
                            WriteMaskedPropertyName(writer, label);
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

            [SkipLocalsInit]
            static void WriteMaskedPropertyName(Utf8JsonWriter writer, string propertyName)
            {
                //use stackallok only for small strings
                if (propertyName.Length < 256)
                {
                    Span<char> chars = stackalloc char[propertyName.Length + 1];
                    chars[0] = '_';
                    propertyName.CopyTo(chars.Slice(1));
                    writer.WritePropertyName(chars);
                }
                else
                {
                    writer.WritePropertyName($"_{propertyName}");
                }
            }
        }

        private static void WriteLogMessageStringValue(Utf8JsonWriter writer, LogEvent logEvent)
        {
            using var bf = new PooledByteBufferWriter();
            using var tw = new Utf8TextWriter(bf);
            logEvent.RenderMessage(tw);
            tw.Flush();
            writer.WriteStringValue(tw.WrittenMemory.Span);
        }

        private static void WriteStructuredValue(Utf8JsonWriter writer, StructureValue structureValue)
        {
            writer.WriteStartObject();

            foreach (var item in structureValue.Properties)
            {
                writer.WritePropertyName(item.Name);
                WritePropertyValue(writer, item.Value);
            }

            writer.WriteEndObject();
        }

        private static void WriteSequenceValue(Utf8JsonWriter writer, SequenceValue sequence)
        {
            writer.WriteStartArray();

            foreach (var item in sequence.Elements)
            {
                WritePropertyValue(writer, item);
            }

            writer.WriteEndArray();
        }

        private static void WriteDictionaryValue(Utf8JsonWriter writer, DictionaryValue value)
        {
            if (value.Elements is null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();

            foreach (var item in value.Elements)
            {
                if (item.Key.WriteAsPropertyName(writer))
                {
                    WritePropertyValue(writer, item.Value);
                }
            }

            writer.WriteEndObject();
        }

        private static void WritePropertyValue(Utf8JsonWriter writer, LogEventPropertyValue? value)
        {
            switch (value)
            {
                case null:
                    writer.WriteNullValue();
                    break;
                case ScalarValue scalarValue:
                    scalarValue.WriteAsValue(writer);
                    break;
                case DictionaryValue dictionaryValue:
                    WriteDictionaryValue(writer, dictionaryValue);
                    break;
                case SequenceValue sequenceValue:
                    WriteSequenceValue(writer, sequenceValue);
                    break;
                case StructureValue structureValue:
                    WriteStructuredValue(writer, structureValue);
                    break;
                default:
                    writer.WriteStringValue(value.ToString());
                    break;
            }
        }

        private void WriteLogMessageAsJson(Utf8JsonWriter destination, LogEvent logEvent)
        {
            using var byteBufferWriter = new PooledByteBufferWriter();
            using var writer = new Utf8JsonWriter(byteBufferWriter);

            writer.WriteStartObject();

            writer.WritePropertyName(_message);

            WriteLogMessageStringValue(writer, logEvent);

            writer.WriteString(_messageTemplate, logEvent.MessageTemplate.Text);

            if (logEvent.Exception is not null)
            {
                writer.WritePropertyName(_exception);
                _exceptionFormatter.Format(writer, logEvent.Exception);
            }

            if (_configurations.EnrichTraceId && logEvent.TraceId.HasValue)
            {
                writer.WriteString(_traceId, logEvent.TraceId.Value.ToString());
            }

            if (_configurations.EnrichSpanId && logEvent.SpanId.HasValue)
            {
                writer.WriteString(_spanId, logEvent.SpanId.Value.ToString());
            }

            foreach (var item in logEvent.Properties)
            {
                writer.WritePropertyName(item.Key);

                WritePropertyValue(writer, item.Value);
            }

            writer.WriteEndObject();

            writer.Flush();

            destination.WriteStringValue(byteBufferWriter.WrittenMemory.Span);
        }



        private void WriteLogs(Utf8JsonWriter writer, IEnumerable<LogEvent> events)
        {
            writer.WritePropertyName(_values);
            writer.WriteStartArray();

            Span<char> buffer = stackalloc char[22];

            foreach (var logEvent in events)
            {
                writer.WriteStartArray();

                logEvent.Timestamp.ToUnixNanoseconds().TryFormat(buffer, out var charsWritten);

                writer.WriteStringValue(buffer.Slice(0, charsWritten));

                WriteLogMessageAsJson(writer, logEvent);

                writer.WriteEndArray();
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
