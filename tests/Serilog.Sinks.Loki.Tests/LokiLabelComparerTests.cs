using Serilog.Events;

namespace Serilog.Sinks.Loki.Tests
{

    public class LokiLabelComparerTests
    {
        private readonly LokiSinkConfigurations _configurations = new LokiSinkConfigurations()
        {
            PropertiesAsLabels = ["label1", "label2"],
            ExposeLogLevelAsLabel = true
        };

        [Fact]
        public void Should_be_equal()
        {
            var comparer = new LokiLogEventComparer(_configurations);
            Serilog.Log.Logger.Information("");
            var logEvent1 = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, new MessageTemplate("test 123", []), new List<LogEventProperty>()
            {
                new LogEventProperty("label1", new ScalarValue("value1")),
                new LogEventProperty("label2", new ScalarValue("value2"))
            });

            var logEvent2 = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, new MessageTemplate("", []), new List<LogEventProperty>()
            {
                new LogEventProperty("label2", new ScalarValue("value2")),
                new LogEventProperty("label3", new ScalarValue("value3")),
                new LogEventProperty("label1", new ScalarValue("value1")),
            });
            var m = logEvent1.RenderMessage();

            Assert.True(comparer.Equals(logEvent1, logEvent2));
        }

        [Theory]
        [InlineData(LogEventLevel.Debug, "value1", "value2")]
        [InlineData(LogEventLevel.Information, "value3", "value2")]
        [InlineData(LogEventLevel.Information, "value1", "value3")]
        [InlineData(LogEventLevel.Information, "value1", null)]
        [InlineData(LogEventLevel.Information, null, "value2")]
        public void Should_not_be_equal(LogEventLevel secondLogLevel, string? secondValue1, string? secondValue2)
        {
            var comparer = new LokiLogEventComparer(_configurations);

            var logEvent1 = new LogEvent(DateTimeOffset.Now, LogEventLevel.Information, null, new MessageTemplate("", []), new List<LogEventProperty>()
            {
                new LogEventProperty("label1", new ScalarValue("value1")),
                new LogEventProperty("label2", new ScalarValue("value2")),

            });

            var logEvent2 = new LogEvent(DateTimeOffset.Now, secondLogLevel, null, new MessageTemplate("", []), new List<LogEventProperty>()
            {
                new LogEventProperty("label1", new ScalarValue(secondValue1)),
                new LogEventProperty("label2", new ScalarValue(secondValue2))
            });

            Assert.False(comparer.Equals(logEvent1, logEvent2));
        }
    }
}