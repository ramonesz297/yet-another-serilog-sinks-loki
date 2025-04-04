﻿// This file is part of the project licensed under the MIT License.
// See the LICENSE file in the project root for more information.


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
