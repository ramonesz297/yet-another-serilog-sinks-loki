using BenchmarkDotNet.Attributes;


namespace Serilog.Sinks.Loki.Benchmark
{
    [ShortRunJob]
    [MemoryDiagnoser]
    public class LokiSinkBenchmark
    {
        public static readonly int intParam = 1;
        public static readonly string stringParam = "string";
        public static readonly double doubleParam = 1.0;
        public static readonly float floatParam = 1.0f;
        public static readonly DateTime dateTimeParam = DateTime.Now;
        public static readonly DateTimeOffset dateTimeOffsetParam = DateTimeOffset.Now;

        public static readonly Object obj = new
        {
            prop1 = 1.1,
            prop2 = "asdasd"
        };

        private Core.Logger _defaultLogger = null!;
        private Core.Logger _logger = null!;

        public static readonly Dictionary<string, object> dictionary = new Dictionary<string, object>()
        {
            {"2",  intParam},
            {"3",  stringParam},
            {"4",  doubleParam},
            {"5",  floatParam},
        };

        private static readonly int[] intArray = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

        [IterationSetup]
        public void IterationSetup()
        {
            _defaultLogger = LoggerConfigurationFactory.Default().CreateLogger();

            _logger = LoggerConfigurationFactory.Optimized().CreateLogger();
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            _defaultLogger.Dispose();
            _logger.Dispose();
        }



        [Benchmark(Baseline = true)]
        public async Task Default()
        {
            for (int i = 0; i < 100; i++)
            {
                _defaultLogger.Debug("Debug message from optimized logger; {intParam} and {stringParam} and {doubleParam} and {floatParam} and {dateTimeParam} and {dateTimeOffsetParam} {@obj} {dictionary} {intArray}",
                                intParam,
                                stringParam,
                                doubleParam,
                                floatParam,
                                dateTimeParam,
                                dateTimeOffsetParam,
                                obj,
                                dictionary,
                                intArray);

                _defaultLogger.Information("Information message from optimized logger; {intParam} and {stringParam} and {doubleParam} and {floatParam} and {dateTimeParam} and {dateTimeOffsetParam} {@obj} {dictionary} {intArray}",
                                intParam,
                                stringParam,
                                doubleParam,
                                floatParam,
                                dateTimeParam,
                                dateTimeOffsetParam,
                                obj,
                                dictionary,
                                intArray);

                _defaultLogger.Debug("Debug message from optimized logger; {intParam} and {stringParam} and {doubleParam} and {floatParam} and {dateTimeParam} and {dateTimeOffsetParam} {@obj} {dictionary} {intArray}",
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

            await Task.Delay(150);
        }

        [Benchmark(Baseline = false)]
        public async Task Optimized()
        {
            for (int i = 0; i < 100; i++)
            {
                _logger.Debug("Debug message from optimized logger; {intParam} and {stringParam} and {doubleParam} and {floatParam} and {dateTimeParam} and {dateTimeOffsetParam} {@obj} {dictionary} {intArray}",
                                intParam,
                                stringParam,
                                doubleParam,
                                floatParam,
                                dateTimeParam,
                                dateTimeOffsetParam,
                                obj,
                                dictionary,
                                intArray);

                _logger.Information("Information message from optimized logger; {intParam} and {stringParam} and {doubleParam} and {floatParam} and {dateTimeParam} and {dateTimeOffsetParam} {@obj} {dictionary} {intArray}",
                                intParam,
                                stringParam,
                                doubleParam,
                                floatParam,
                                dateTimeParam,
                                dateTimeOffsetParam,
                                obj,
                                dictionary,
                                intArray);

                _logger.Debug("Debug message from optimized logger; {intParam} and {stringParam} and {doubleParam} and {floatParam} and {dateTimeParam} and {dateTimeOffsetParam} {@obj} {dictionary} {intArray}",
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

            await Task.Delay(150);
        }
    }
}

