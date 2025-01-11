using Serilog.Events;
using System.Diagnostics.CodeAnalysis;

namespace Serilog.Sinks.Loki.Internal
{
    internal class LokiLogEventComparer : IEqualityComparer<LogEvent>
    {
        private readonly LokiSinkConfigurations _configurations;

        internal LokiLogEventComparer(LokiSinkConfigurations configurations)
        {
            _configurations = configurations;
        }

        public bool Equals(LogEvent? x, LogEvent? y)
        {
            if (x == y)
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            if (_configurations.HandleLogLevelAsLabel && x.Level != y.Level)
            {
                return false;
            }

            for (int i = _configurations.PropertiesAsLabels.Length - 1; i >= 0; i--)
            {
                var label = _configurations.PropertiesAsLabels[i];

                if (!(x.Properties.TryGetValue(label, out var xValue) ^ y.Properties.TryGetValue(label, out var yValue)))
                {
                    if (xValue == yValue)
                    {
                        continue;
                    }

                    if (xValue is null || yValue is null)
                    {
                        return false;
                    }

                    if (!xValue.Equals(yValue))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(LogEvent obj)
        {
            var hasCode = new HashCode();

            if (_configurations.HandleLogLevelAsLabel)
            {
                hasCode.Add(obj.Level);
            }

            for (int i = _configurations.PropertiesAsLabels.Length - 1; i >= 0; i--)
            {
                var label = _configurations.PropertiesAsLabels[i];
                if (obj.Properties.TryGetValue(label, out var x))
                {
                    hasCode.Add(label);
                    hasCode.Add(x);
                }
            }

            return hasCode.ToHashCode();
        }
    }
}
