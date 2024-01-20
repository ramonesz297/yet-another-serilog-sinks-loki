namespace Serilog.Sinks.Loki
{
    /// <summary>
    /// Describes global loki label.
    /// All <see cref="LokiLabel"/> will added as label to each log event
    /// </summary>
    public readonly struct LokiLabel : IEqualityComparer<LokiLabel>, IEquatable<LokiLabel>
    {
        /// <summary>
        /// Key of label
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Value of label
        /// </summary>
        public string Value { get; }


        /// <summary>
        /// Creates new instance of <see cref="LokiLabel"/>
        /// </summary>
        /// <param name="key">Key of label</param>
        /// <param name="value">Value of label</param>
        public LokiLabel(string key, string value)
        {
            Key = key;
            Value = value;
        }
        /// <inheritdoc/>
        public bool Equals(LokiLabel x, LokiLabel y)
        {
            return x.Equals(y);
        }
        /// <inheritdoc/>
        public int GetHashCode(LokiLabel obj)
        {
            return obj.GetHashCode();
        }
        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is LokiLabel label && Equals(label);
        }
        /// <inheritdoc/>
        public bool Equals(LokiLabel other)
        {
            return Key == other.Key &&
                   Value == other.Value;
        }
        /// <inheritdoc/>
        public static bool operator ==(LokiLabel left, LokiLabel right)
        {
            return left.Equals(right);
        }
        /// <inheritdoc/>
        public static bool operator !=(LokiLabel left, LokiLabel right)
        {
            return !(left == right);
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Value);
        }
    }
}
