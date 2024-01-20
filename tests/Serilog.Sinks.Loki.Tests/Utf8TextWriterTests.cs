﻿using System.Globalization;
using System.Text;

namespace Serilog.Sinks.Loki.Tests
{
    public class Utf8TextWriterTests
    {

        private readonly PooledByteBufferWriter _pooledByteBufferWriter = new PooledByteBufferWriter();
        private readonly Utf8TextWriter _writer;

        public Utf8TextWriterTests()
        {
            _writer = new(_pooledByteBufferWriter);
        }


        private string GetWrittenString()
        {
            _writer.Flush();
            var bytes = _pooledByteBufferWriter.WrittenMemory.ToArray();
            return Encoding.UTF8.GetString(bytes);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Should_write_bool(bool value)
        {
            _writer.Write(value);

            Assert.Equal(value.ToString().ToLower(), GetWrittenString());
        }


        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void Should_write_int(int value)
        {
            _writer.Write(value);

            Assert.Equal(value.ToString(), GetWrittenString());
        }


        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(uint.MaxValue)]
        public void Should_write_uint(uint value)
        {
            _writer.Write(value);

            Assert.Equal(value.ToString(), GetWrittenString());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(long.MaxValue)]
        [InlineData(long.MinValue)]
        public void Should_write_long(long value)
        {
            _writer.Write(value);

            Assert.Equal(value.ToString(), GetWrittenString());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(ulong.MaxValue)]
        public void Should_write_ulong(ulong value)
        {
            _writer.Write(value);

            Assert.Equal(value.ToString(), GetWrittenString());
        }


        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(ulong.MaxValue)]
        public void Should_write_float(float value)
        {
            _writer.Write(value);

            Assert.Equal(value.ToString(CultureInfo.InvariantCulture), GetWrittenString());
        }

        [Theory]
        [InlineData(0d)]
        [InlineData(-1d)]
        [InlineData(double.MaxValue)]
        public void Should_write_double(double value)
        {
            _writer.Write(value);

            Assert.Equal(value.ToString(CultureInfo.InvariantCulture), GetWrittenString());
        }


        [Fact]
        public void Should_write_decimal()
        {
            decimal value = Decimal.MaxValue;

            _writer.Write(value);

            Assert.Equal(value.ToString(CultureInfo.InvariantCulture), GetWrittenString());
        }


        [Fact]
        public void Should_write_char_array()
        {
            char[] value = "value".ToCharArray();

            _writer.Write(value, 0, value.Length);

            Assert.Equal("value", GetWrittenString());
        }

        [Fact]
        public void Should_write_sb()
        {
            var sb = new StringBuilder()
                .Append("val1")
                .AppendLine("line")
                .Append("val2");

            _writer.Write(sb);

            Assert.Equal(sb.ToString(), GetWrittenString());
        }


    }
}