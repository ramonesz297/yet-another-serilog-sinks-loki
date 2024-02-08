using System;
using System.Buffers;

namespace Serilog.Sinks.Loki.Internal
{
    /// <summary>
    /// <para>
    /// Pooled buffer writer
    /// </para>
    /// <para>
    /// implementation taken from System.Text.Json.PooledByteBufferWriter
    /// </para>
    /// <para>
    /// <see href="https://github.com/dotnet/runtime/blob/72db600a20d581fdc6776edce1863bcf8da0b1cd/src/libraries/Common/src/System/Text/Json/PooledByteBufferWriter.cs"/>
    /// </para>
    /// </summary>
    internal sealed class PooledByteBufferWriter : IBufferWriter<byte>, IDisposable
    {
        private byte[] _buffer;

        private int _index;

        internal PooledByteBufferWriter(int initialCapacity = 256)
        {
            _buffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
        }

        public ReadOnlyMemory<byte> WrittenMemory => _buffer.AsMemory(0, _index);

        public void Advance(int count)
        {
            _index += count;
        }

        public void Clear()
        {
            _index = 0;
            _buffer.AsSpan().Clear();
        }

        public void Dispose()
        {
            Clear();
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = null!;
            GC.SuppressFinalize(this);
        }

        public Memory<byte> GetMemory(int sizeHint = 256)
        {
            EnshureCapacity(sizeHint);
            return _buffer.AsMemory(_index);
        }

        public Span<byte> GetSpan(int sizeHint = 256)
        {
            EnshureCapacity(sizeHint);
            return _buffer.AsSpan(_index);
        }

        private void EnshureCapacity(int sizeHint)
        {
            var length = _buffer.Length;

            if (sizeHint <= length - _index)
            {
                return;
            }

            int requiredLength = Math.Max(sizeHint, length);

            int newSize = length + requiredLength;

            byte[] oldBuffer = _buffer;
            var oldBufferSpan = oldBuffer.AsSpan();

            _buffer = ArrayPool<byte>.Shared.Rent(newSize);

            oldBufferSpan.CopyTo(_buffer);

            oldBufferSpan.Clear();

            ArrayPool<byte>.Shared.Return(oldBuffer);
        }
    }
}
