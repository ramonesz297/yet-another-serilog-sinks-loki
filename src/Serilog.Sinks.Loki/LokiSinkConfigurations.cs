namespace Serilog.Sinks.Loki
{

    /// <summary>
    /// class contains all configurations for Loki sink
    /// </summary>
    public class LokiSinkConfigurations
    {
        /// <summary>
        /// Base uri of loki server. Required property
        /// </summary>
        public Uri Url { get; set; } = null!;

        /// <summary>
        /// Global static labels,
        /// Will be added to all log events
        /// </summary>
        public LokiLabel[] Labels { get; set; } = [];

        /// <summary>
        /// Basic authentication credentials
        /// </summary>
        public LokiCredentials? Credentials { get; set; }

        /// <summary>
        /// Log event properties to be added as labels.
        /// Property matching are case sensetive.
        /// </summary>
        public string[] PropertiesAsLabels { get; set; } = [];

        /// <summary>
        /// When <see langword="true"/> then <see cref="Serilog.Events.LogEvent.Level"/> will be added as label.
        /// Default is <see langword="true"/>
        /// </summary>
        public bool HandleLogLevelAsLabel { get; set; } = true;

        /// <summary>
        /// Loki tenant name. When provided <code>X-Scope-OrgID</code> header will be added to each request
        /// </summary>
        public string? Tenant { get; set; }

    }
}
