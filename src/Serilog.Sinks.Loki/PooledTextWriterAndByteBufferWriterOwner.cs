using System.Collections.Concurrent;

namespace Serilog.Sinks.Loki
{
    internal class PooledTextWriterAndByteBufferWriterOwner
    {
        private readonly ConcurrentBag<Utf8TextWriter> _utf8TextWriters = [];
        private readonly ConcurrentBag<PooledByteBufferWriter> _byteBufferWriters = [];

        public static readonly PooledTextWriterAndByteBufferWriterOwner Instance = new();

        private PooledTextWriterAndByteBufferWriterOwner()
        {

        }

        public PooledByteBufferWriter RentBufferWriter(int sizeHint = 256)
        {
            if (_byteBufferWriters.TryTake(out var byteBufferWriter))
            {
                return byteBufferWriter;
            }

            return new PooledByteBufferWriter(sizeHint);
        }

        public Utf8TextWriter RentWriter(int sizeHint = 256)
        {
            if (_utf8TextWriters.TryTake(out var textWriter))
            {
                return textWriter;
            }

            var pooledByteBufferWriter = new PooledByteBufferWriter(sizeHint);

            return new Utf8TextWriter(pooledByteBufferWriter);
        }

        public void Return(Utf8TextWriter textWriter)
        {
            textWriter.Clear();

            _utf8TextWriters.Add(textWriter);
        }

        public void Return(PooledByteBufferWriter byteBufferWriter)
        {
            byteBufferWriter.Clear();

            _byteBufferWriters.Add(byteBufferWriter);
        }
    }
}
