// This file is part of the project licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using BenchmarkDotNet.Attributes;
using Serilog.Core;

namespace Serilog.Sinks.Loki.Benchmark
{
    /// <summary>
    /// Black-box comparison between both public Serilog sink APIs.
    /// Uses a local stub HTTP transport to remove real Loki/server variability.
    /// Includes logger disposal so sink flush cost is part of the measurement.
    /// </summary>
    [HideColumns("RatioSD", "Job", "Error", "StdDev")]
    [MemoryDiagnoser]
    public class BlackBoxSinkComparisonBenchmark
    {
        private static readonly int _intParam = 1;
        private static readonly string _stringParam = """
            Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium.
            """;
        private static readonly double _doubleParam = 1.0;
        private static readonly float _floatParam = 1.0f;
        private static readonly DateTime _dateTimeParam = new(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        private static readonly DateTimeOffset _dateTimeOffsetParam = new(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);

        private static readonly object _obj = new
        {
            prop1 = 1.1,
            prop2 = "payload",
            prop3 = 1,
            prop4 = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            prop5 = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero),
            prop6 = new
            {
                child1 = "test test test",
                child2 = "test test test"
            }
        };

        private static readonly Dictionary<string, object> _dictionary = new()
        {
            { "2", _intParam },
            { "3", _stringParam },
            { "4", _doubleParam },
            { "5", _floatParam },
            { "6", "payload-1" },
            { "7", "payload-2" },
            { "8", "payload-3" },
            { "9", "payload-4" },
            { "10", "payload-5" },
        };

        private static readonly int[] _intArray = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

        private LokiStubServer _server = null!;

        [Params(100, 1000)]
        public int Count;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _server = LokiStubServer.Start();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            _server.ResetMetrics();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _server.Dispose();
        }

        [Benchmark]
        public void GrafanaLoki_WriteAndFlush()
        {
            using var logger = LoggerConfigurationFactory.GrafanaLokiBlackBox(_server.BaseUri.ToString()).CreateLogger();

            for (int i = 0; i < Count; i++)
            {
                PerformLogs(logger);
            }
        }

        [Benchmark(Baseline = true)]
        public void YetAnotherLoki_WriteAndFlush()
        {
            using var logger = LoggerConfigurationFactory.YetAnotherLokiBlackBox(_server.BaseUri.ToString()).CreateLogger();

            for (int i = 0; i < Count; i++)
            {
                PerformLogs(logger);
            }
        }

        private static void PerformLogs(Logger logger)
        {
            logger.Debug(
                "Debug message; {intParam} and {stringParam} and {doubleParam} and {floatParam} and {dateTimeParam} and {dateTimeOffsetParam} {@obj} {dictionary} {intArray}",
                _intParam,
                _stringParam,
                _doubleParam,
                _floatParam,
                _dateTimeParam,
                _dateTimeOffsetParam,
                _obj,
                _dictionary,
                _intArray);

            logger.Information(
                "Information message; {intParam} and {stringParam} and {doubleParam} and {floatParam} and {dateTimeParam} and {dateTimeOffsetParam} {@obj} {dictionary} {intArray}",
                _intParam,
                _stringParam,
                _doubleParam,
                _floatParam,
                _dateTimeParam,
                _dateTimeOffsetParam,
                _obj,
                _dictionary,
                _intArray);

            logger.Debug(
                "Debug message; {intParam} and {stringParam} and {doubleParam} and {floatParam} and {dateTimeParam} and {dateTimeOffsetParam} {@obj} {dictionary} {intArray}",
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