namespace Serilog.Sinks.Loki.Internal
{
    internal static class DateTimeOffsetExtensions
    {
        internal static long ToUnixNanoseconds(this DateTimeOffset offset)
        {
#if NET7_0_OR_GREATER
            return offset.ToUnixTimeMilliseconds() * 1000000 +
             offset.Microsecond * 1000 +
             offset.Nanosecond;
#else
            return offset.ToUnixTimeMilliseconds() * 1000000;
#endif

        }
    }
}
