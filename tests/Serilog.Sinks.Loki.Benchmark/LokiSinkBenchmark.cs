// This file is part of the project licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using BenchmarkDotNet.Attributes;
using Serilog.Core;

namespace Serilog.Sinks.Loki.Benchmark
{
    /// <summary>
    /// Compares logging throughput between YetAnotherLoki and Grafana.Loki sinks.
    /// Loggers are created once in GlobalSetup and reused across iterations to measure
    /// steady-state logging performance, not logger construction/disposal overhead.
    /// Requires a Loki-compatible HTTP endpoint at localhost:8080.
    /// </summary>
    [HideColumns("RatioSD", "Job", "Error", "StdDev")]
    [ShortRunJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net10_0)]
    [MemoryDiagnoser]
    public class SinkLokiComparisonBenchmark
    {
        private static readonly int _intParam = 1;
        private static readonly string _stringParam = """
            «Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam eaque ipsa, quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt, explicabo. Nemo enim ipsam voluptatem, quia voluptas sit, aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos, qui ratione voluptatem sequi nesciunt, neque porro quisquam est, qui dolorem ipsum, quia dolor sit, amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt, ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur? Quis autem vel eum iure reprehenderit, qui in ea voluptate velit esse, quam nihil molestiae consequatur, vel illum, qui dolorem eum fugiat, quo voluptas nulla pariatur? [33] At vero eos et accusamus et iusto odio dignissimos ducimus, qui blanditiis praesentium voluptatum deleniti atque corrupti, quos dolores et quas molestias excepturi sint, obcaecati cupiditate non provident, similique sunt in culpa, qui officia deserunt mollitia animi, id est laborum et dolorum fuga. Et harum quidem rerum facilis est et expedita distinctio. Nam libero tempore, cum soluta nobis est eligendi optio, cumque nihil impedit, quo minus id, quod maxime placeat, facere possimus, omnis voluptas assumenda est, omnis dolor repellendus. Temporibus autem quibusdam et aut officiis debitis aut rerum necessitatibus saepe eveniet, ut et voluptates repudiandae sint et molestiae non recusandae. Itaque earum rerum hic tenetur a sapiente delectus, ut aut reiciendis voluptatibus maiores alias consequatur aut perferendis doloribus asperiores repellat.»
            """;
        private static readonly double _doubleParam = 1.0;
        private static readonly float _floatParam = 1.0f;
        private static readonly DateTime _dateTimeParam = DateTime.Now;
        private static readonly DateTimeOffset _dateTimeOffsetParam = DateTimeOffset.Now;

        private static readonly object _obj = new
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

        private static readonly Dictionary<string, object> _dictionary = new()
        {
            {"2",  _intParam},
            {"3",  _stringParam},
            {"4",  _doubleParam},
            {"5",  _floatParam},
            {"6", "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium 1"},
            {"7", "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium 2"},
            {"8", "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium 3"},
            {"9", "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium 4"},
            {"10", "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium 5"},
        };

        private static readonly int[] _intArray = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

        private Logger _grafanaLokiLogger = null!;
        private Logger _yetAnotherLokiLogger = null!;

        [GlobalSetup]
        public async Task GlobalSetup()
        {
            await WebAppHostHelpers.WarmUpApplication("http://localhost:8080");

            _grafanaLokiLogger = LoggerConfigurationFactory.Serilog_Sinks_Grafana_Loki("http://localhost:8080").CreateLogger();
            _yetAnotherLokiLogger = LoggerConfigurationFactory.YetAnotherLoki("http://localhost:8080").CreateLogger();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _grafanaLokiLogger.Dispose();
            _yetAnotherLokiLogger.Dispose();
        }

        [Params(100, 1000)]
        public int Count;

        [Benchmark]
        public void Serilog_Sinks_Grafana_Loki()
        {
            for (int i = 0; i < Count; i++)
            {
                PerformLogs(_grafanaLokiLogger);
            }
        }

        [Benchmark(Baseline = true)]
        public void YetAnotherLoki()
        {
            for (int i = 0; i < Count; i++)
            {
                PerformLogs(_yetAnotherLokiLogger);
            }
        }

        private static void PerformLogs(Logger logger)
        {
            logger.Debug("Debug message from optimized logger; {intParam} and {stringParam} and {doubleParam} and {floatParam} and {dateTimeParam} and {dateTimeOffsetParam} {@obj} {dictionary} {intArray}",
                                   _intParam,
                                   _stringParam,
                                   _doubleParam,
                                   _floatParam,
                                   _dateTimeParam,
                                   _dateTimeOffsetParam,
                                   _obj,
                                   _dictionary,
                                   _intArray);

            logger.Information("Information message from optimized logger; {intParam} and {stringParam} and {doubleParam} and {floatParam} and {dateTimeParam} and {dateTimeOffsetParam} {@obj} {dictionary} {intArray}",
                            _intParam,
                            _stringParam,
                            _doubleParam,
                            _floatParam,
                            _dateTimeParam,
                            _dateTimeOffsetParam,
                            _obj,
                            _dictionary,
                            _intArray);

            logger.Debug("Debug message from optimized logger; {intParam} and {stringParam} and {doubleParam} and {floatParam} and {dateTimeParam} and {dateTimeOffsetParam} {@obj} {dictionary} {intArray}",
                            _intParam,
                            _stringParam,
                            _doubleParam,
                            _floatParam,
                            _dateTimeParam,
                            _dateTimeOffsetParam,
                            _obj,
                            _dictionary,
                            _intArray);
        }
    }
}

