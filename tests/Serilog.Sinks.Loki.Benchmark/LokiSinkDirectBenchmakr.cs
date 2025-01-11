using BenchmarkDotNet.Attributes;
using Serilog.Events;
using Serilog.Sinks.Loki.Internal;
using System.Net.Http;


namespace Serilog.Sinks.Loki.Benchmark
{
    [ShortRunJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net481)]
    [ShortRunJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net90)]
    [MemoryDiagnoser]
    public class YetAnotherLokiBenchmakr
    {

        private LokiSink _sink = null!;

        private LogEvent[] _events_1 = [];

        private ILogger _logger = null!;

        private static readonly int[] intArray = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        private static readonly int intParam = 1;
        public static readonly string stringParam = """
            «Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam eaque ipsa, quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt, explicabo. Nemo enim ipsam voluptatem, quia voluptas sit, aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos, qui ratione voluptatem sequi nesciunt, neque porro quisquam est, qui dolorem ipsum, quia dolor sit, amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt, ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur? Quis autem vel eum iure reprehenderit, qui in ea voluptate velit esse, quam nihil molestiae consequatur, vel illum, qui dolorem eum fugiat, quo voluptas nulla pariatur? [33] At vero eos et accusamus et iusto odio dignissimos ducimus, qui blanditiis praesentium voluptatum deleniti atque corrupti, quos dolores et quas molestias excepturi sint, obcaecati cupiditate non provident, similique sunt in culpa, qui officia deserunt mollitia animi, id est laborum et dolorum fuga. Et harum quidem rerum facilis est et expedita distinctio. Nam libero tempore, cum soluta nobis est eligendi optio, cumque nihil impedit, quo minus id, quod maxime placeat, facere possimus, omnis voluptas assumenda est, omnis dolor repellendus. Temporibus autem quibusdam et aut officiis debitis aut rerum necessitatibus saepe eveniet, ut et voluptates repudiandae sint et molestiae non recusandae. Itaque earum rerum hic tenetur a sapiente delectus, ut aut reiciendis voluptatibus maiores alias consequatur aut perferendis doloribus asperiores repellat.»
            """;
        private static readonly double doubleParam = 1.0;
        private static readonly float floatParam = 1.0f;
        private static readonly DateTime dateTimeParam = DateTime.Now;
        private static readonly DateTimeOffset dateTimeOffsetParam = DateTimeOffset.Now;
        private static readonly object obj = new
        {
            prop1 = 1.1,
            prop2 = "asdasd"
        };
        private static readonly Dictionary<int, string> keyValuePairs = new Dictionary<int, string>()
        {
            {1, "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium 1"},
            {2, "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium 2"},
            {3, "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium 3"},
            {4, "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium 4"},
            {5, "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium 5"},
        };

        private static readonly object[] _setOfParameters_1 = [
intParam, stringParam, doubleParam, floatParam, dateTimeParam, dateTimeOffsetParam, obj, keyValuePairs, intArray
            ];

        [GlobalSetup]
        public async Task GlobalSetup()
        {
            _logger = new LoggerConfiguration().CreateLogger();

            _events_1 = [
                _logger.Create(LogEventLevel.Information, "Info: Simple text only 1"),
                _logger.Create(LogEventLevel.Information, "Info: Simple text only 2"),
                _logger.Create(LogEventLevel.Information, "Info: Simple text only 3"),
                _logger.Create(LogEventLevel.Information, "Info: Simple text only 4"),
                _logger.Create(LogEventLevel.Information, "Info: Simple text only 5"),
                _logger.Create(LogEventLevel.Debug, "Debug: Simple text only 1"),
                _logger.Create(LogEventLevel.Debug, "Debug: Simple text only 2"),
                _logger.Create(LogEventLevel.Debug, "Debug: Simple text only 3"),
                _logger.Create(LogEventLevel.Debug, "Debug: Simple text only 4"),
                _logger.Create(LogEventLevel.Debug, "Debug: Simple text only 5"),
                _logger.Create(LogEventLevel.Debug, "Debug: Parametrized text {0}; {1}; {2}; {3}; {4}; {5}; {6}; {7}; {8}",propertyValues: _setOfParameters_1),
                _logger.Create(LogEventLevel.Debug, "Debug: Parametrized text {0}; {1}; {2}; {3}; {4}; {5}; {6}; {7}; {8}",propertyValues: _setOfParameters_1),
                _logger.Create(LogEventLevel.Debug, "Debug: Parametrized text {0}; {1}; {2}; {3}; {4}; {5}; {6}; {7}; {8}",propertyValues: _setOfParameters_1),
                _logger.Create(LogEventLevel.Debug, "Debug: Parametrized text {0}; {1}; {2}; {3}; {4}; {5}; {6}; {7}; {8}",propertyValues: _setOfParameters_1),
                _logger.Create(LogEventLevel.Debug, "Debug: Parametrized text {0}; {1}; {2}; {3}; {4}; {5}; {6}; {7}; {8}",propertyValues: _setOfParameters_1),
                ];

            await WebAppHostHelpers.WarmUpApplication("http://localhost:8080");

            _sink = new LokiSink(new()
            {
                Credentials = new("admin", "admin"),
                EnrichSpanId = true,
                EnrichTraceId = true,
                HandleLogLevelAsLabel = true,
                Labels = [new("app", "sinks2")],
                Url = new Uri("http://localhost:8080")
            }, new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:8080"),
            }, new DefaultLokiExceptionFormatter());
        }

        [Benchmark]
        public async Task EmitBatchAsync()
        {
            await _sink.EmitBatchAsync(_events_1);
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _sink.Dispose();
        }
    }
}

