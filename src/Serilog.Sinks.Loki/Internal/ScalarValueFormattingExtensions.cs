using Serilog.Events;
using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;

namespace Serilog.Sinks.Loki.Internal
{
    internal static class ScalarValueFormattingExtensions
    {
        internal static void WriteAsValue(this ScalarValue scalarValue, Utf8JsonWriter writer)
        {
            object? value = scalarValue.Value;

            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            switch (value)
            {
                case int:
                case short:
                case ushort:
                case byte:
                    writer.WriteNumberValue((int)value);
                    break;
                case uint:
                    writer.WriteNumberValue((uint)value);
                    break;
                case string:
                    writer.WriteStringValue((string)value);
                    break;
                case float floatValue:
                    writer.WriteNumberValue(floatValue);
                    break;
                case double doubleValue:
                    writer.WriteNumberValue(doubleValue);
                    break;
                case long longValue:
                    writer.WriteNumberValue(longValue);
                    break;
                case ulong ulongValue:
                    writer.WriteNumberValue(ulongValue);
                    break;
                case DateTime dateTimeValue:
                    writer.WriteStringValue(dateTimeValue);
                    break;
                case DateTimeOffset dateTimeOffsetValue:
                    writer.WriteStringValue(dateTimeOffsetValue);
                    break;
                default:
                    writer.WriteStringValue(value.ToString());
                    break;
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
            const int stackallocThreshold = 256;

            object? value = scalarValue.Value;

            if (value is null)
            {
                return false;
            }

            if (value is string stringValue)
            {
                Write(writer, stringValue.AsSpan(), writeAsProperty);
                return true;
            }
            else
            {
                int count = GetCount(value);

                if (count <= 0)
                {
                    Write(writer, value.ToString().AsSpan(), writeAsProperty);
                    return false;
                }

                var maxLength = checked(count);

                byte[]? rentedBuffer = null;

                Span<byte> span = maxLength <= stackallocThreshold ? stackalloc byte[maxLength] : (rentedBuffer = ArrayPool<byte>.Shared.Rent(maxLength));

                try
                {
                    if (TryFormat(value, span, out int charsWritten))
                    {
                        Write(writer, span.Slice(0, charsWritten), writeAsProperty);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                finally
                {
                    if (rentedBuffer != null)
                    {
                        ArrayPool<byte>.Shared.Return(rentedBuffer);
                    }
                }
            }
        }

        private static void Write(Utf8JsonWriter writer, ReadOnlySpan<char> data, bool writeAsProperty)
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

        private static void Write(Utf8JsonWriter writer, ReadOnlySpan<byte> data, bool writeAsProperty)
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

        internal static bool WriteAsStringValue(this ScalarValue scalarValue, Utf8JsonWriter writer)
        {
            return WriteAs(scalarValue, writer, false);
        }

        internal static bool WriteAsPropertyName(this ScalarValue scalarValue, Utf8JsonWriter writer)
        {
            return WriteAs(scalarValue, writer, true);
        }

        internal static bool TryFormat(object value, Span<byte> destination, out int bytesWritten)
        {
            if (value is null or string)
            {
                bytesWritten = 0;
                return false;
            }
            if (value is byte _byte)
            {
                return Utf8Formatter.TryFormat(_byte, destination, out bytesWritten);
            }
            else if (value is short _short)
            {
                return Utf8Formatter.TryFormat(_short, destination, out bytesWritten);
            }
            else if (value is short _ushort)
            {
                return Utf8Formatter.TryFormat(_ushort, destination, out bytesWritten);
            }
            else if (value is bool _bool)
            {
                return Utf8Formatter.TryFormat(_bool, destination, out bytesWritten, 'l');
            }
            else if (value is int _int)
            {
                return Utf8Formatter.TryFormat(_int, destination, out bytesWritten);
            }
            else if (value is uint _uint)
            {
                return Utf8Formatter.TryFormat(_uint, destination, out bytesWritten);
            }
            else if (value is long _long)
            {
                return Utf8Formatter.TryFormat(_long, destination, out bytesWritten);
            }
            else if (value is ulong _ulong)
            {
                return Utf8Formatter.TryFormat(_ulong, destination, out bytesWritten);
            }
            else if (value is float _float)
            {
                return Utf8Formatter.TryFormat(_float, destination, out bytesWritten);
            }
            else if (value is Guid guid)
            {
                return Utf8Formatter.TryFormat(guid, destination, out bytesWritten);
            }
            else if (value is double _double)
            {
                return Utf8Formatter.TryFormat(_double, destination, out bytesWritten);
            }
            else if (value is decimal _decimal)
            {
                return Utf8Formatter.TryFormat(_decimal, destination, out bytesWritten);
            }
            else if (value is DateTimeOffset dateTimeOffset)
            {
                return Utf8Formatter.TryFormat(dateTimeOffset, destination, out bytesWritten, 'O');
            }
            else if (value is DateTime dateTime)
            {
                return Utf8Formatter.TryFormat(dateTime, destination, out bytesWritten, 'O');
            }
            else if (value is TimeSpan timeSpan)
            {
                return Utf8Formatter.TryFormat(timeSpan, destination, out bytesWritten);
            }
            else
            {
                bytesWritten = 0;
                return false;
            }
        }

        //internal static bool TryFormat(this ScalarValue scalarValue, Span<char> destination, out int charsWritten)
        //{
        //    if (scalarValue.Value is null or string)
        //    {
        //        charsWritten = 0;
        //        return false;
        //    }

        //    if (scalarValue.Value is ISpanFormattable formattable)
        //    {
        //        ReadOnlySpan<char> format = scalarValue.Value switch
        //        {
        //            DateTimeOffset => "O",
        //            DateTime => "O",
        //            _ => default
        //        };

        //        return formattable.TryFormat(destination, out charsWritten, format, null);
        //    }
        //    else
        //    {
        //        charsWritten = 0;
        //        return false;
        //    }

        //}

        internal static int GetCount(object? value)
        {
            if (value is null)
            {
                return 0;
            }

            return value switch
            {
                int => 11,
                uint => 10,
                double => 33,
                float => 33,
                decimal => 22,
                long => 20,
                short => 6,
                ushort => 5,
                ulong => 20,
                bool booleanValue => booleanValue ? 4 : 5,
                DateTimeOffset => 36,
                DateTime => 36,
                TimeSpan => 26,
                Guid => 36,
                Enum => GetCount(Enum.GetUnderlyingType(value.GetType())),
                _ => -1
            };
        }



    }
}
