﻿// This file is part of the project licensed under the MIT License.
// See the LICENSE file in the project root for more information.


using Serilog.Events;

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
        /// When <see langword="true"/> then <see cref="LogEvent.Level"/> will be added as label.
        /// Default is <see langword="true"/>
        /// </summary>
        public bool HandleLogLevelAsLabel { get; set; } = true;

        /// <summary>
        /// Loki tenant name. When provided <code>X-Scope-OrgID</code> header will be added to each request
        /// </summary>
        public string? Tenant { get; set; }

        /// <summary>
        /// if <see langword="true"/> then <see cref="LogEvent.TraceId"/> from will be added to each log event 
        /// as 'TraceId' json property
        /// </summary>
        public bool EnrichTraceId { get; set; } = false;

        /// <summary>
        /// if <see langword="true"/> then <see cref="LogEvent.SpanId"/> from will be added to each log event 
        /// as 'SpanId' json property
        /// </summary>
        public bool EnrichSpanId { get; set; } = false;

    }
}
