// This file is part of the project licensed under the MIT License.
// See the LICENSE file in the project root for more information.


using System.Buffers.Text;
using System.Text;

namespace Serilog.Sinks.Loki.Internal
{
    internal sealed class Utf8TextWriter : TextWriter
    {
        private readonly PooledByteBufferWriter _pooledByteBufferWriter;

        private int _index = 0;

        private Memory<byte> _buffer;
        public override Encoding Encoding => Encoding.UTF8;

        internal Utf8TextWriter(PooledByteBufferWriter pooledByteBufferWriter)
        {
            _pooledByteBufferWriter = pooledByteBufferWriter;
            _buffer = _pooledByteBufferWriter.GetMemory();
        }


        private Span<byte> Cursor
        {
            get
            {
                return _buffer.Span.Slice(_index);
            }
        }

        public ReadOnlyMemory<byte> WrittenMemory
        {
            get
            {
                return _pooledByteBufferWriter.WrittenMemory;
            }
        }

        public override void Write(bool value)
        {
            EnsureCapacity(value ? 4 : 5);

            Utf8Formatter.TryFormat(value, Cursor, out var bytesWritten, 'l');

            _index += bytesWritten;
        }

        public override void Write(int value)
        {
            const int maxLen = 11;

            EnsureCapacity(maxLen);

            Utf8Formatter.TryFormat(value, Cursor, out var bytesWritten);

            _index += bytesWritten;
        }

        public override void Write(uint value)
        {
            const int maxLen = 10;

            EnsureCapacity(maxLen);

            Utf8Formatter.TryFormat(value, Cursor, out var bytesWritten);

            _index += bytesWritten;
        }

        public override void Write(long value)
        {
            const int maxLen = 20;

            EnsureCapacity(maxLen);

            Utf8Formatter.TryFormat(value, Cursor, out var bytesWritten);

            _index += bytesWritten;
        }

        public override void Write(ulong value)
        {
            const int maxLen = 20;
            EnsureCapacity(maxLen);

            Utf8Formatter.TryFormat(value, Cursor, out var bytesWritten);

            _index += bytesWritten;
        }


        public override void Write(float value)
        {
            const int maxLen = 32;

            EnsureCapacity(maxLen);

            Utf8Formatter.TryFormat(value, Cursor, out var bytesWritten);

            _index += bytesWritten;
        }

        public override void Write(double value)
        {
            const int maxLen = 32;

            EnsureCapacity(maxLen);

            Utf8Formatter.TryFormat(value, Cursor, out var bytesWritten);

            _index += bytesWritten;
        }

        public override void Write(char[] buffer, int index, int count)
        {
            WriteCore(buffer.AsSpan().Slice(index, count));
        }

        public override void Write(decimal value)
        {
            const int maxLen = 64;

            EnsureCapacity(maxLen);

            Utf8Formatter.TryFormat(value, Cursor, out var bytesWritten);

            _index += bytesWritten;
        }

#if NETCOREAPP

        public override void Write(ReadOnlySpan<char> buffer)
        {
            var maxLen = Encoding.GetByteCount(buffer);

            EnsureCapacity(maxLen);

            _index += Encoding.GetBytes(buffer, Cursor);
        }
#endif

        public override void WriteLine()
        {
            Write(Environment.NewLine);
        }


        public override void Write(string? value)
        {
            if (value is null)
            {
                return;
            }

            WriteCore(value.AsSpan());
        }

        private void WriteCore(ReadOnlySpan<char> buffer)
        {
            if (buffer.IsEmpty)
            {
                return;
            }

#if NETCOREAPP
            var maxLen = Encoding.GetByteCount(buffer);
            EnsureCapacity(maxLen);
            _index += Encoding.GetBytes(buffer, Cursor);
#else
            unsafe
            {
                fixed (char* t = buffer)
                {
                    var maxLen = Encoding.GetByteCount(t, buffer.Length);
                    EnsureCapacity(maxLen);
                    Span<byte> dest = Cursor.Slice(0, maxLen);

                    fixed (byte* d = dest)
                    {
                        _index += Encoding.GetBytes(t, buffer.Length, d, dest.Length);
                    }
                }
            }
#endif
        }

        public override void Write(char[]? buffer)
        {
            var source = buffer.AsSpan();
            WriteCore(source);
        }

#if NETCOREAPP
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

#endif
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _index = 0;
                _buffer = default;
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
            _pooledByteBufferWriter.Advance(_index);
            _index = 0;
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
                WriteCore([value]);
            }
        }
    }
}
