using Serilog.Events;
using System.Text.Json;

namespace Serilog.Sinks.Loki.Internal
{
    internal static class ScalarValueFormattingExtensions
    {
        internal static void WriteAsValue(this ScalarValue scalarValue, Utf8JsonWriter writer)
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
            else if (scalarValue.Value is uint uintValue)
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

        /// <summary>
        /// writes scalar value as property name or as value
        /// </summary>
        /// <param name="scalarValue">Scalar value</param>
        /// <param name="writer">Output json writer</param>
        /// <param name="writeAsProperty">indicates destination</param>
        /// <returns>returns <see langword="true"/> if value was written, otherwise - <see langword="false"/></returns>
        private static bool WriteAs(this ScalarValue scalarValue, Utf8JsonWriter writer, bool writeAsProperty)
        {
            int count = GetCharsCount(scalarValue);

            if (count == -1)
            {
                if (scalarValue.Value is string stringValue)
                {
                    Write(writer, stringValue, writeAsProperty);
                    return true;
                }
                else if (scalarValue.Value is object value)
                {
                    var stringToWrite = value.ToString();
                    if (!string.IsNullOrEmpty(stringToWrite))
                    {
                        Write(writer, stringToWrite, writeAsProperty);

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else if (count == 0)
            {
                return false;
            }
            else
            {
                Span<char> span = stackalloc char[count];
                if (scalarValue.TryFormat(span, out int charsWritten))
                {
                    Write(writer, span.Slice(0, charsWritten), writeAsProperty);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            static void Write(Utf8JsonWriter writer, ReadOnlySpan<char> data, bool writeAsProperty)
            {
                if (writeAsProperty)
                {
                    writer.WritePropertyName(data);
                }
                else
                {
                    writer.WriteStringValue(data);
                }
            }
        }

        internal static bool WriteAsStringValue(this ScalarValue scalarValue, Utf8JsonWriter writer)
        {
            return WriteAs(scalarValue, writer, false);
        }

        internal static bool WriteAsPropertyName(this ScalarValue scalarValue, Utf8JsonWriter writer)
        {
            return WriteAs(scalarValue, writer, true);
        }

        internal static bool TryFormat(this ScalarValue scalarValue, Span<char> destination, out int charsWritten)
        {
            if (scalarValue.Value is null or string)
            {
                charsWritten = 0;
                return false;
            }


            if (scalarValue.Value is int intValue)
            {
                return intValue.TryFormat(destination, out charsWritten);
            }
            else if (scalarValue.Value is uint uintValue)
            {
                return uintValue.TryFormat(destination, out charsWritten);
            }
            else if (scalarValue.Value is double doubleValue)
            {
                return doubleValue.TryFormat(destination, out charsWritten);
            }
            else if (scalarValue.Value is float floatValue)
            {
                return floatValue.TryFormat(destination, out charsWritten);
            }
            else if (scalarValue.Value is decimal decimalValue)
            {
                return decimalValue.TryFormat(destination, out charsWritten);
            }
            else if (scalarValue.Value is long longValue)
            {
                return longValue.TryFormat(destination, out charsWritten);
            }
            else if (scalarValue.Value is ulong ulongValue)
            {
                return ulongValue.TryFormat(destination, out charsWritten);
            }
            else if (scalarValue.Value is bool booleanValue)
            {
                if (booleanValue)
                {
                    "true".CopyTo(destination);
                    charsWritten = 4;
                    return true;
                }
                else
                {
                    "false".CopyTo(destination);
                    charsWritten = 5;
                    return true;
                }
            }
            else if (scalarValue.Value is DateTimeOffset dateTimeOffsetValue)
            {
                return dateTimeOffsetValue.TryFormat(destination, out charsWritten, "O");
            }
            else if (scalarValue.Value is DateTime dateTimeValue)
            {
                return dateTimeValue.TryFormat(destination, out charsWritten, "O");
            }
            else
            {
                charsWritten = 0;
                return false;
            }

        }
        internal static int GetCharsCount(ScalarValue scalarValue)
        {
            if (scalarValue?.Value is null)
            {
                return 0;
            }

            return scalarValue.Value switch
            {
                int => 11,
                uint => 10,
                double => 33,
                float => 33,
                decimal => 22,
                long => 20,
                ulong => 20,
                bool booleanValue => booleanValue ? 4 : 5,
                DateTimeOffset => 36,
                DateTime => 36,
                _ => -1
            };
        }


    }
}
