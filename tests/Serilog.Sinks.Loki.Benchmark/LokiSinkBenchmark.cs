using BenchmarkDotNet.Attributes;
using Serilog.Core;
using System.Security.Cryptography;


namespace Serilog.Sinks.Loki.Benchmark
{
    [HideColumns("RatioSD", "Job", "Error", "StdDev")]
    [ShortRunJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net481)]
    [ShortRunJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net90)]
    [MemoryDiagnoser]
    public class SinkLokiComparationBenchmakr
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

        [GlobalSetup]
        public async Task GlobalSetup()
        {
            await WebAppHostHelpers.WarmUpApplication("http://localhost:8080");
        }


        [Params(50, 100, 1000, 2000)]
        public int Count;

        [Benchmark()]
        public void Serilog_Sinks_Grafana_Loki()
        {
            using var _defaultLogger = LoggerConfigurationFactory.Serilog_Sinks_Grafana_Loki("http://localhost:8080").CreateLogger();
            for (int i = 0; i < Count; i++)
            {
                PerformLoggs(_defaultLogger);
            }
        }

        [Benchmark()]
        public void Empty()
        {
            using var _logger = LoggerConfigurationFactory.Empty().CreateLogger();
            for (int i = 0; i < Count; i++)
            {
                PerformLoggs(_logger);
            }
        }

        [Benchmark(Baseline = true)]
        public void YetAnotherLoki()
        {
            using var _logger = LoggerConfigurationFactory.YetAnotherLoki("http://localhost:8080").CreateLogger();
            for (int i = 0; i < Count; i++)
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

