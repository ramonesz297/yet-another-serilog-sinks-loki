using Serilog.Configuration;
using Serilog.Sinks.Loki.Internal;

namespace Serilog.Sinks.Loki
{
    /// <summary>
    /// Extensions for <see cref="LoggerSinkConfiguration"/> to add Loki sink(s) to the logger configuration.
    /// </summary>
    public static class LokiSinkExtensions
    {
        /// <summary>
        /// Adds a sink that will send log events to Grafana Loki.
        /// <para>
        /// <code>
        ///  Log.Logger = new LoggerConfiguration()
        ///         .WriteTo.Loki(new LokiSinkConfigurations()
        ///         {
        ///                Credentials = new LokiCredentials("login here ", "password here "),
        ///                Url = new Uri("uri to loki server here"),
        ///                PropertiesAsLabels = ["userId"], 
        ///                Labels = 
        ///                [
        ///                    new LokiLabel("app", "loki"),
        ///                ]
        ///            })
        ///            .CreateLogger();
        /// </code>
        /// </para>
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
        /// <param name="exceptionFormatter">Custom formatter for exceptions</param>
        /// <param name="retryTimeLimit">The maximum time that the sink will keep retrying failed batches for. The default is ten minutes. Lower
        /// this value to reduce buffering and backpressure in high-load scenarios.
        /// </param>
        /// <returns></returns>
        public static LoggerConfiguration Loki(this LoggerSinkConfiguration loggerConfiguration,
                                               LokiSinkConfigurations configurations,
                                               int batchSizeLimit = 1000,
                                               TimeSpan? period = null,
                                               int queueLimit = 100000,
                                               bool eagerlyEmitFirstEvent = true,
                                               HttpClient? httpClient = null,
                                               ILokiExceptionFormatter? exceptionFormatter = null,
                                               TimeSpan? retryTimeLimit = null)
        {

            ArgumentNullException.ThrowIfNull(configurations, nameof(configurations));
            ArgumentNullException.ThrowIfNull(configurations.Url, "configurations.Url");
            ArgumentNullException.ThrowIfNull(configurations.Labels, "configurations.Labels");
            ArgumentNullException.ThrowIfNull(configurations.PropertiesAsLabels, "configurations.PropertiesAsLabels");

            var sink = new LokiSink(configurations, httpClient ?? new(), exceptionFormatter ?? new DefaultLokiExceptionFormatter());

            return loggerConfiguration.Sink(sink, new BatchingOptions()
            {
                BufferingTimeLimit = period.GetValueOrDefault(TimeSpan.FromSeconds(2)),
                BatchSizeLimit = batchSizeLimit,
                EagerlyEmitFirstEvent = eagerlyEmitFirstEvent,
                QueueLimit = queueLimit,
                RetryTimeLimit = retryTimeLimit.GetValueOrDefault(TimeSpan.FromMinutes(10))
            });
        }
    }
}
