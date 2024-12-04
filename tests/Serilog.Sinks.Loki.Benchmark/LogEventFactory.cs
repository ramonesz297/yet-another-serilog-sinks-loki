using Serilog.Events;


namespace Serilog.Sinks.Loki.Benchmark
{
    public static class LogEventFactory
    {
        public static LogEvent Create(this ILogger logger,
                                      LogEventLevel level,
                                      string messageTemplate,
                                      Exception? ex = null,
                                      params object[] propertyValues)
        {
            if (!logger.BindMessageTemplate(messageTemplate, propertyValues, out var template, out var properties))
            {
                throw new InvalidOperationException();
            }

            var traceId = System.Diagnostics.ActivityTraceId.CreateRandom();

            var spanId = System.Diagnostics.ActivitySpanId.CreateRandom();

            return new LogEvent(DateTimeOffset.Now, level, ex, template, properties, traceId, spanId);
        }
    }
}

