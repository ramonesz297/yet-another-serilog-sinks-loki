using System.Buffers.Text;
using System.Text;

namespace Serilog.Sinks.Loki.Internal
{
    internal sealed class Utf8TextWriter : TextWriter
    {
        private readonly PooledByteBufferWriter _pooledByteBufferWriter;

        private int _index = 0;

        private Memory<byte> _buffer;
        public override Encoding Encoding { get; } = Encoding.UTF8;

        internal Utf8TextWriter(PooledByteBufferWriter pooledByteBufferWriter)
        {
            _pooledByteBufferWriter = pooledByteBufferWriter;
            _buffer = _pooledByteBufferWriter.GetMemory();
        }

        private Span<byte> Cursor => _buffer.Span.Slice(_index);

        public ReadOnlyMemory<byte> WrittenMemory => _pooledByteBufferWriter.WrittenMemory;

        public override void Write(bool value)
        {
            ReadOnlySpan<byte> ut8Value = value ? "true"u8 : "false"u8;

            EnsureCapacity(ut8Value.Length);

            ut8Value.CopyTo(Cursor);

            _index += ut8Value.Length;
        }

        public override void Write(int value)
        {
            var maxLen = 11;

            EnsureCapacity(maxLen);

            Utf8Formatter.TryFormat(value, Cursor, out var bytesWritten);

            _index += bytesWritten;
        }

        public override void Write(uint value)
        {
            var maxLen = 10;
            EnsureCapacity(maxLen);

            Utf8Formatter.TryFormat(value, Cursor, out var bytesWritten);

            _index += bytesWritten;
        }

        public override void Write(long value)
        {
            var maxLen = 20;
            EnsureCapacity(maxLen);

            Utf8Formatter.TryFormat(value, Cursor, out var bytesWritten);

            _index += bytesWritten;
        }

        public override void Write(ulong value)
        {
            var maxLen = 20;
            EnsureCapacity(maxLen);

            Utf8Formatter.TryFormat(value, Cursor, out var bytesWritten);

            _index += bytesWritten;
        }


        public override void Write(float value)
        {
            var maxLen = 32;

            EnsureCapacity(maxLen);

            Utf8Formatter.TryFormat(value, Cursor, out var bytesWritten);

            _index += bytesWritten;
        }

        public override void Write(double value)
        {
            var maxLen = 32;

            EnsureCapacity(maxLen);

            Utf8Formatter.TryFormat(value, Cursor, out var bytesWritten);

            _index += bytesWritten;
        }

        public override void Write(char[] buffer, int index, int count)
        {
            Write(buffer.AsSpan().Slice(index, count));
        }

        public override void Write(decimal value)
        {
            var maxLen = 64;

            EnsureCapacity(maxLen);

            Utf8Formatter.TryFormat(value, Cursor, out var bytesWritten);

            _index += bytesWritten;
        }


        public override void Write(ReadOnlySpan<char> buffer)
        {
            var maxLen = Encoding.GetByteCount(buffer);

            EnsureCapacity(maxLen);

            _index += Encoding.GetBytes(buffer, Cursor);
        }

        public override void WriteLine()
        {
            Write(Environment.NewLine.AsSpan());
        }


        public override void Write(string? value)
        {
            if (value is null)
            {
                return;
            }

            var maxLen = Encoding.GetByteCount(value);

            EnsureCapacity(maxLen);

            _index += Encoding.GetBytes(value.AsSpan(), Cursor);

        }

        public override void Write(char[]? buffer)
        {
            Write(buffer.AsSpan());
        }


        public override void Write(StringBuilder? value)
        {
            if (value is null)
            {
                return;
            }

            foreach (var item in value.GetChunks())
            {
                Write(item.Span);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _index = 0;
                _buffer = default;
                _pooledByteBufferWriter.Dispose();
            }
        }

        private void EnsureCapacity(int sizeHint)
        {
            int needed = _index + sizeHint;

            if (needed <= _buffer.Length)
            {
                return;
            }

            FlushBuffer(needed);
        }

        public void Clear()
        {
            _pooledByteBufferWriter.Clear();
            _index = 0;
            _buffer = _pooledByteBufferWriter.GetMemory();
        }

        private void FlushBuffer(int sizeHint)
        {
            _pooledByteBufferWriter.Advance(_index);
            _buffer = _pooledByteBufferWriter.GetMemory(sizeHint);
            _index = 0;
        }

        public override void Flush()
        {
            FlushBuffer(0);
        }

        public override void Write(char value)
        {
            if (value <= byte.MaxValue)
            {
                EnsureCapacity(1);
                Cursor[0] = Convert.ToByte(value);
                _index++;
            }
            else
            {
                var maxLen = Encoding.GetByteCount([value]);
                EnsureCapacity(maxLen);
                _index += Encoding.GetBytes([value], Cursor);
            }

        }


    }
}
