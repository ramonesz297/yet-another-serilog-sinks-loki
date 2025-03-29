// This file is part of the project licensed under the MIT License.
// See the LICENSE file in the project root for more information.


using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.Loki.Internal;
using System.Text;

namespace Serilog.Sinks.Loki.Tests
{
    public class LokiHttpContentTests
    {
        private static readonly DateTimeOffset _date = new(2021, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private readonly ILokiExceptionFormatter _defaultExceptionFormatter = new DefaultLokiExceptionFormatter();

        private readonly MessageTemplateParser _messageTemplateParser = new();

        private LokiMessageWriter Create(LokiSinkConfigurations? configurations = null)
        {
            configurations ??= new LokiSinkConfigurations();

            LokiLogEventComparer comparer = new LokiLogEventComparer(configurations);

            return new LokiMessageWriter(configurations, comparer, _defaultExceptionFormatter);
        }

        private string StringifyJsonPayload(MemoryStream stream)
        {
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        [Fact]
        public async Task ShouldSerializeContent()
        {
            var logWriter = Create();
            var mtp = _messageTemplateParser;
            var messmtpageTemplate = _messageTemplateParser.Parse("log wihout parameters");

            var log = new LogEvent(_date, LogEventLevel.Debug, null, messmtpageTemplate, []);
            var log1_1 = new LogEvent(_date, LogEventLevel.Information, null, mtp.Parse("log #1.1"), []);
            var log1_2 = new LogEvent(_date, LogEventLevel.Information, null, mtp.Parse("log #1.2"), []);
            var log2_1 = new LogEvent(_date, LogEventLevel.Debug, null, mtp.Parse("log #2.1"), []);
            var log2_2 = new LogEvent(_date, LogEventLevel.Debug, null, mtp.Parse("log #2.2"), []);

            using var ms = new MemoryStream();
            var content = LokiPushContent.Create(logWriter, [log, log1_1, log1_2, log2_1, log2_2]);
            await content.CopyToAsync(ms);
            var r = StringifyJsonPayload(ms);
            await Verify(r);
        }
    }
}