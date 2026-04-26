// This file is part of the project licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using System.Buffers;
using System.Diagnostics;

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
    /// This file includes code from the following repository:
    /// <see href="https://github.com/dotnet/runtime/blob/72db600a20d581fdc6776edce1863bcf8da0b1cd/src/libraries/Common/src/System/Text/Json/PooledByteBufferWriter.cs"/>
    /// The original code is licensed under the MIT License.
    /// </para>
    /// </summary>
    internal sealed class PooledByteBufferWriter : IBufferWriter<byte>, IDisposable
    {
        internal PooledByteBufferWriter(int initialCapacity = 256)
        {
            Buffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
        }

        public ReadOnlyMemory<byte> WrittenMemory => Buffer.AsMemory(0, WrittenCount);

        public byte[] Buffer { get; private set; }

        public int WrittenCount { get; private set; }

        ///<inheritdoc/>
        public void Advance(int count)
        {
            WrittenCount += count;
        }

        public void Clear()
        {
            WrittenCount = 0;
            Buffer.AsSpan().Clear();
        }

        public void Dispose()
        {
            if (Buffer == null)
            {
                return;
            }

            Clear();
            ArrayPool<byte>.Shared.Return(Buffer);
            Buffer = null!;
        }

        public Memory<byte> GetMemory(int sizeHint = 256)
        {
            EnshureCapacity(sizeHint);
            return Buffer.AsMemory(WrittenCount);
        }

        public Span<byte> GetSpan(int sizeHint = 256)
        {
            EnshureCapacity(sizeHint);
            return Buffer.AsSpan(WrittenCount);
        }

        private void EnshureCapacity(int sizeHint)
        {
            Debug.Assert(sizeHint > 0);

            var length = Buffer.Length;

            if (sizeHint <= length - WrittenCount)
            {
                return;
            }

            int requiredLength = Math.Max(sizeHint, length);

            int newSize = length + requiredLength;

            byte[] oldBuffer = Buffer;

            var oldBufferSpan = oldBuffer.AsSpan();

            Buffer = ArrayPool<byte>.Shared.Rent(newSize);

            oldBufferSpan.CopyTo(Buffer);
            oldBufferSpan.Clear();
            ArrayPool<byte>.Shared.Return(oldBuffer);
        }
    }
}
