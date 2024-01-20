using System.Collections.Concurrent;

namespace Serilog.Sinks.Loki
{
    internal sealed class PooledTextWriterAndByteBufferWriterOwner : IDisposable
    {
        private readonly ConcurrentBag<Utf8TextWriter> _utf8TextWriters = [];
        private readonly ConcurrentBag<PooledByteBufferWriter> _byteBufferWriters = [];

        internal PooledTextWriterAndByteBufferWriterOwner()
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

        public void Dispose()
        {
            foreach (var item in _utf8TextWriters)
            {
                item.Dispose();
            }

            foreach (var item in _byteBufferWriters)
            {
                item.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}
