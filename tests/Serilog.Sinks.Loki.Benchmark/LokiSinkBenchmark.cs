using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.dotMemory;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Loki.Internal;


namespace Serilog.Sinks.Loki.Benchmark
{

    [SimpleJob()]
    [MemoryDiagnoser]
    public class LokiSinkDirectBenchmakr
    {
        private WebApplication _app = null!;

        private HttpClient _client = null!;

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

            _app = WebAppHostFactory.Create(8080);

            _client = await WebAppHostFactory.RunAppAsync(_app);

            _sink = new LokiSink(new()
            {
                Credentials = new("admin", "admin"),
                EnrichSpanId = true,
                EnrichTraceId = true,
                HandleLogLevelAsLabel = true,
                Labels = [new("app", "sinks2")],
                Url = _client.BaseAddress!
            }, _client, new DefaultLokiExceptionFormatter());
        }

        [Benchmark]
        public async Task YetAnotherLoki()
        {
            await _sink.EmitBatchAsync(_events_1);
        }

        [GlobalCleanup]
        public async Task GlobalCleanup()
        {
            _sink.Dispose();
            await _app.DisposeAsync();
        }
    }


    [SimpleJob]
    [MemoryDiagnoser]
    public class LokiSinkBenchmark
    {
        public static readonly int intParam = 1;
        public static readonly string stringParam = """
            «Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam eaque ipsa, quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt, explicabo. Nemo enim ipsam voluptatem, quia voluptas sit, aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos, qui ratione voluptatem sequi nesciunt, neque porro quisquam est, qui dolorem ipsum, quia dolor sit, amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt, ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur? Quis autem vel eum iure reprehenderit, qui in ea voluptate velit esse, quam nihil molestiae consequatur, vel illum, qui dolorem eum fugiat, quo voluptas nulla pariatur? [33] At vero eos et accusamus et iusto odio dignissimos ducimus, qui blanditiis praesentium voluptatum deleniti atque corrupti, quos dolores et quas molestias excepturi sint, obcaecati cupiditate non provident, similique sunt in culpa, qui officia deserunt mollitia animi, id est laborum et dolorum fuga. Et harum quidem rerum facilis est et expedita distinctio. Nam libero tempore, cum soluta nobis est eligendi optio, cumque nihil impedit, quo minus id, quod maxime placeat, facere possimus, omnis voluptas assumenda est, omnis dolor repellendus. Temporibus autem quibusdam et aut officiis debitis aut rerum necessitatibus saepe eveniet, ut et voluptates repudiandae sint et molestiae non recusandae. Itaque earum rerum hic tenetur a sapiente delectus, ut aut reiciendis voluptatibus maiores alias consequatur aut perferendis doloribus asperiores repellat.»
            """;
        public static readonly double doubleParam = 1.0;
        public static readonly float floatParam = 1.0f;
        public static readonly DateTime dateTimeParam = DateTime.Now;
        public static readonly DateTimeOffset dateTimeOffsetParam = DateTimeOffset.Now;

        public static readonly Object obj = new
        {
            prop1 = 1.1,
            prop2 = "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium",
            prop3 = 1,
            prop4 = DateTime.Now,
            prop5 = DateTimeOffset.Now,
            prop6 = new
            {
                child1 = "test test test",
                child2 = "test test test"
            }
        };

        public static readonly Dictionary<string, object> dictionary = new Dictionary<string, object>()
        {
            {"2",  intParam},
            {"3",  stringParam},
            {"4",  doubleParam},
            {"5",  floatParam},
            {"6", "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium 1"},
            {"7", "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium 2"},
            {"8", "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium 3"},
            {"9", "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium 4"},
            {"10", "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium 5"},
        };

        private static readonly int[] intArray = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

        private WebApplication _app1 = null!;
        private WebApplication _app2 = null!;

        [GlobalSetup]
        public async Task GlobalSetup()
        {
            _app1 = WebAppHostFactory.Create(8080);
            _app2 = WebAppHostFactory.Create(8081);

            await WebAppHostFactory.RunAppAsync(_app1);
            await WebAppHostFactory.RunAppAsync(_app2);
        }

        [GlobalCleanup]
        public async Task GlobalCleanup()
        {
            await _app1.DisposeAsync();
            await _app2.DisposeAsync();
        }

        [Benchmark(Baseline = true)]
        public async Task Default()
        {
            await using var _defaultLogger = LoggerConfigurationFactory.Default("http://localhost:8081").CreateLogger();
            for (int i = 0; i < 100; i++)
            {
                PerformLoggs(_defaultLogger);
            }

        }

        [Benchmark()]
        public async Task Optimized()
        {
            await using var _logger = LoggerConfigurationFactory.Optimized("http://localhost:8080").CreateLogger();
            for (int i = 0; i < 100; i++)
            {
                PerformLoggs(_logger);
            }
        }

        private void PerformLoggs(Logger logger)
        {
            logger.Debug("Debug message from optimized logger; {intParam} and {stringParam} and {doubleParam} and {floatParam} and {dateTimeParam} and {dateTimeOffsetParam} {@obj} {dictionary} {intArray}",
                                   intParam,
                                   stringParam,
                                   doubleParam,
                                   floatParam,
                                   dateTimeParam,
                                   dateTimeOffsetParam,
                                   obj,
                                   dictionary,
                                   intArray);

            logger.Information("Information message from optimized logger; {intParam} and {stringParam} and {doubleParam} and {floatParam} and {dateTimeParam} and {dateTimeOffsetParam} {@obj} {dictionary} {intArray}",
                            intParam,
                            stringParam,
                            doubleParam,
                            floatParam,
                            dateTimeParam,
                            dateTimeOffsetParam,
                            obj,
                            dictionary,
                            intArray);

            logger.Debug("Debug message from optimized logger; {intParam} and {stringParam} and {doubleParam} and {floatParam} and {dateTimeParam} and {dateTimeOffsetParam} {@obj} {dictionary} {intArray}",
                            intParam,
                            stringParam,
                            doubleParam,
                            floatParam,
                            dateTimeParam,
                            dateTimeOffsetParam,
                            obj,
                            dictionary,
                            intArray);
        }
    }
}

