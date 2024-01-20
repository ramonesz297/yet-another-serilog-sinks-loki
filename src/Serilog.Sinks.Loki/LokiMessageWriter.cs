using Serilog.Events;
using System.Text;
using System.Text.Json;

namespace Serilog.Sinks.Loki
{
    internal class LokiMessageWriter
    {
        private readonly LokiSinkConfigurations _configurations;
        private readonly LokiLogEventComparer _comparer;
        private readonly PooledTextWriterAndByteBufferWriterOwner _bufferOwner;
        internal LokiMessageWriter(LokiSinkConfigurations configurations, PooledTextWriterAndByteBufferWriterOwner bufferOwner, LokiLogEventComparer comparer)
        {
            _configurations = configurations;
            _comparer = comparer;
            _bufferOwner = bufferOwner;
        }

        private static ReadOnlySpan<byte> MapLogLevel(LogEventLevel logLevel)
        {
            return logLevel switch
            {
                LogEventLevel.Verbose => "trace"u8,
                LogEventLevel.Debug => "debug"u8,
                LogEventLevel.Information => "info"u8,
                LogEventLevel.Warning => "warning"u8,
                LogEventLevel.Error => "error"u8,
                LogEventLevel.Fatal => "critical"u8,
                _ => "unknown"u8
            };
        }


        public void Write(Utf8JsonWriter writer, IEnumerable<LogEvent> events)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("streams"u8);
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
            writer.WritePropertyName("stream"u8);
            writer.WriteStartObject();

            if (_configurations.HandleLogLevelAsLabel)
            {
                writer.WritePropertyName("level"u8);
                writer.WriteStringValue(MapLogLevel(head.Level));
            }

            for (int i = 0; i < _configurations.Labels.Length; i++)
            {
                var label = _configurations.Labels[i];

                if (label.Key == "level" && _configurations.HandleLogLevelAsLabel)
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
                        if (label == "level" && _configurations.HandleLogLevelAsLabel)
                        {
                            writer.WritePropertyName("_level"u8);
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

                        WriteScalarValue(writer, scalarValue);
                    }
                }
            }

            writer.WriteEndObject();

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

        private void WriteLogMessageStringValue(Utf8JsonWriter writer, LogEvent logEvent)
        {
            var tw = _bufferOwner.RentWriter();

            logEvent.RenderMessage(tw);

            tw.Flush();

            writer.WriteStringValue(tw.WrittenMemory.Span);

            _bufferOwner.Return(tw);


        }
        
        private bool WriteScalarPropertyName(Utf8JsonWriter writer, ScalarValue scalarValue)
        {
            if (scalarValue.Value is null)
            {
                return false;
            }

            int writedChars;
            if (scalarValue.Value is int intValue)
            {
                Span<char> chars = stackalloc char[11];
                intValue.TryFormat(chars, out writedChars);
                writer.WritePropertyName(chars.Slice(0, writedChars));
            }
            else if (scalarValue.Value is double doubleValue)
            {
                Span<char> chars = stackalloc char[33];
                doubleValue.TryFormat(chars, out writedChars);
                writer.WritePropertyName(chars.Slice(0, writedChars));
            }
            else if (scalarValue.Value is float floatValue)
            {
                Span<char> chars = stackalloc char[33];
                floatValue.TryFormat(chars, out writedChars);
                writer.WritePropertyName(chars.Slice(0, writedChars));
            }
            else if (scalarValue.Value is decimal decimalValue)
            {
                Span<char> chars = stackalloc char[22];
                decimalValue.TryFormat(chars, out writedChars);
                writer.WritePropertyName(chars.Slice(0, writedChars));
            }
            else if (scalarValue.Value is string stringValue)
            {
                writer.WritePropertyName(stringValue);
            }
            else if (scalarValue.Value is long longValue)
            {
                Span<char> chars = stackalloc char[20];
                longValue.TryFormat(chars, out writedChars);
                writer.WritePropertyName(chars.Slice(0, writedChars));
            }
            else if (scalarValue.Value is bool booleanValue)
            {
                writer.WritePropertyName(booleanValue ? "true" : "false");
            }
            else if (scalarValue.Value is DateTimeOffset dateTimeOffsetValue)
            {
                Span<char> chars = stackalloc char[36];
                dateTimeOffsetValue.TryFormat(chars, out writedChars, "O");
                writer.WritePropertyName(chars);
            }
            else if (scalarValue.Value is DateTime dateTimeValue)
            {
                Span<char> chars = stackalloc char[36];
                dateTimeValue.TryFormat(chars, out writedChars, "O");
                writer.WritePropertyName(chars);
            }
            else if (scalarValue.Value != null)
            {
                var value = scalarValue.Value.ToString();

                if (value is null)
                {
                    return false;
                }

                writer.WritePropertyName(value);
            }
            else
            {
                return false;
            }

            return true;
        }
        private static void WriteScalarValue(Utf8JsonWriter writer, ScalarValue scalarValue)
        {
            if (scalarValue.Value is null)
            {
                writer.WriteNullValue();
                return;
            }

            if (scalarValue.Value is int intValue)
            {
                writer.WriteNumberValue(intValue);
            }
            if (scalarValue.Value is uint uintValue)
            {
                writer.WriteNumberValue(uintValue);
            }
            else if (scalarValue.Value is string stringValue)
            {
                writer.WriteStringValue(stringValue);
            }
            else if (scalarValue.Value is float floatValue)
            {
                writer.WriteNumberValue(floatValue);
            }
            else if (scalarValue.Value is double doubleValue)
            {
                writer.WriteNumberValue(doubleValue);
            }
            else if (scalarValue.Value is long longValue)
            {
                writer.WriteNumberValue(longValue);
            }
            else if (scalarValue.Value is ulong ulongValue)
            {
                writer.WriteNumberValue(ulongValue);
            }
            else if (scalarValue.Value is DateTime dateTimeValue)
            {
                writer.WriteStringValue(dateTimeValue);
            }
            else if (scalarValue.Value is DateTimeOffset dateTimeOffsetValue)
            {
                writer.WriteStringValue(dateTimeOffsetValue);
            }
            else
            {
                writer.WriteStringValue(scalarValue.Value.ToString());
            }
        }


        private void WriteStructuredValue(Utf8JsonWriter writer, StructureValue structureValue)
        {
            writer.WriteStartObject();

            foreach (var item in structureValue.Properties)
            {
                writer.WritePropertyName(item.Name);
                WritePropertyValue(writer, item.Value);
            }

            writer.WriteEndObject();
        }

        private void WriteSequenceValue(Utf8JsonWriter writer, SequenceValue sequence)
        {

            writer.WriteStartArray();

            foreach (var item in sequence.Elements)
            {
                WritePropertyValue(writer, item);
            }

            writer.WriteEndArray();
        }

        private void WriteDictionaryValue(Utf8JsonWriter writer, DictionaryValue value)
        {
            if (value.Elements is null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();

            foreach (var item in value.Elements)
            {
                if (WriteScalarPropertyName(writer, item.Key))
                {
                    WritePropertyValue(writer, item.Value);
                }
            }

            writer.WriteEndObject();
        }

        private void WritePropertyValue(Utf8JsonWriter writer, LogEventPropertyValue? value)
        {
            if (value is null)
            {
                writer.WriteNullValue();
            }
            else if (value is ScalarValue scalarValue)
            {
                WriteScalarValue(writer, scalarValue);
            }
            else if (value is DictionaryValue dictionaryValue)
            {
                WriteDictionaryValue(writer, dictionaryValue);
            }
            else if (value is SequenceValue sequenceValue)
            {
                WriteSequenceValue(writer, sequenceValue);
            }
            else if (value is StructureValue structureValue)
            {
                WriteStructuredValue(writer, structureValue);
            }
            else
            {
                writer.WriteStringValue(value.ToString());
            }
        }


        private static void WriteException(Utf8JsonWriter writer, Exception exception)
        {
            if (exception is null)
            {
                return;
            }

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
                WriteException(writer, exception.InnerException);
            }

            writer.WriteEndObject();
        }

        private void WriteLogMessageAsJson(Utf8JsonWriter destination, LogEvent logEvent)
        {
            var byteBufferWriter = _bufferOwner.RentBufferWriter();

            using var writer = new Utf8JsonWriter(byteBufferWriter);

            writer.WriteStartObject();

            writer.WritePropertyName("Message"u8);

            WriteLogMessageStringValue(writer, logEvent);

            writer.WriteString("MessageTemplate"u8, logEvent.MessageTemplate.Text);

            if (logEvent.Exception is not null)
            {
                writer.WritePropertyName("Exception"u8);
                WriteException(writer, logEvent.Exception);
            }

            foreach (var item in logEvent.Properties)
            {
                writer.WritePropertyName(item.Key);

                WritePropertyValue(writer, item.Value);
            }

            writer.WriteEndObject();

            writer.Flush();

            destination.WriteStringValue(byteBufferWriter.WrittenMemory.Span);

            _bufferOwner.Return(byteBufferWriter);
        }



        private void WriteLogs(Utf8JsonWriter writer, IEnumerable<LogEvent> events)
        {
            writer.WritePropertyName("values"u8);
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
