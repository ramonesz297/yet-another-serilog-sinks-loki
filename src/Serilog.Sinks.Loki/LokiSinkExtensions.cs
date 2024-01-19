using Serilog.Configuration;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.Loki
{
    public static class LokiSinkExtensions
    {
        /// <summary>
        /// Adds a sink that will send log events to Grafana Loki.
        /// </summary>
        /// <param name="loggerConfiguration"></param>
        /// <param name="configurations">common sink configurations</param>
        /// <param name="batchSizeLimit">The maximum number of events to include in a single batch. The default is 1000.</param>
        /// <param name="period">The time to wait between checking for event batches. The default is two seconds.</param>
        /// <param name="queueLimit">Maximum number of events to hold in the sink's internal queue, or null for an unbounded queue. The default is 100000.</param>
        /// <param name="eagerlyEmitFirstEvent">
        /// Eagerly emit a batch containing the first received event, regardless of the target
        /// batch size or batching time. This helps with perceived "liveness" when running/debugging
        /// applications interactively. The default is true.
        ///  </param>
        /// <param name="httpClient">Custom HttpClient instance</param>
        /// <returns></returns>
        public static LoggerConfiguration Loki(this LoggerSinkConfiguration loggerConfiguration,
                                                   LokiSinkConfigurations configurations,
                                                   int batchSizeLimit = 100,
                                                   TimeSpan? period = null,
                                                   int queueLimit = 10000,
                                                   bool eagerlyEmitFirstEvent = true,
                                                   HttpClient? httpClient = null)
        {

            ArgumentNullException.ThrowIfNull(configurations, nameof(configurations));
            ArgumentNullException.ThrowIfNull(configurations.Url, "configurations.Url");
            ArgumentNullException.ThrowIfNull(configurations.Labels, "configurations.Labels");
            ArgumentNullException.ThrowIfNull(configurations.PropertiesAsLabels, "configurations.PropertiesAsLabels");

            var p = new PeriodicBatchingSink(new LokiSink(configurations, httpClient ?? new()), new()
            {
                Period = period.GetValueOrDefault(TimeSpan.FromSeconds(2)),
                BatchSizeLimit = batchSizeLimit,
                EagerlyEmitFirstEvent = eagerlyEmitFirstEvent,
                QueueLimit = queueLimit,
            });

            return loggerConfiguration.Sink(p);
        }
    }
}
