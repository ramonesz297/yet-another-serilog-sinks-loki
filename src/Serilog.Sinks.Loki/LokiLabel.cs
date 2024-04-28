namespace Serilog.Sinks.Loki
{
    /// <summary>
    /// Describes global loki label.
    /// All <see cref="LokiLabel"/> will added as label to each log event
    /// </summary>
    public class LokiLabel : IEqualityComparer<LokiLabel>, IEquatable<LokiLabel>
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
        public bool Equals(LokiLabel? x, LokiLabel? y)
        {
            return x == y;
        }

        /// <inheritdoc/>
        public int GetHashCode(LokiLabel obj)
        {
            return obj.GetHashCode();
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return Equals(obj as LokiLabel);
        }


        /// <inheritdoc/>
        public bool Equals(LokiLabel? other)
        {
            return Equals(this, other);
        }

        /// <inheritdoc/>
        public static bool operator ==(LokiLabel? left, LokiLabel? right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Key == right.Key && left.Value == right.Value;
        }

        /// <inheritdoc/>
        public static bool operator !=(LokiLabel? left, LokiLabel? right)
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
