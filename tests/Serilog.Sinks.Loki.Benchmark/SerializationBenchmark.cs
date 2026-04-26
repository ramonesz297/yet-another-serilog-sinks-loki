// This file is part of the project licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using BenchmarkDotNet.Attributes;
using Serilog.Events;
using Serilog.Sinks.Loki.Internal;
using System.Text.Json;

namespace Serilog.Sinks.Loki.Benchmark
{
    /// <summary>
    /// Benchmarks JSON serialization of log events (LokiMessageWriter) in isolation,
    /// without any HTTP or network overhead. No external services required.
    /// </summary>
    [ShortRunJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net10_0)]
    [MemoryDiagnoser]
    [HideColumns("RatioSD", "Job", "Error", "StdDev")]
    public class SerializationBenchmark
    {
        private LokiMessageWriter _writer = null!;
        private LogEvent[] _simpleEvents = [];
        private LogEvent[] _complexEvents = [];

        private static readonly int _intParam = 1;
        private static readonly string _stringParam = "Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium";
        private static readonly double _doubleParam = 1.0;
        private static readonly float _floatParam = 1.0f;
        private static readonly DateTime _dateTimeParam = new(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        private static readonly DateTimeOffset _dateTimeOffsetParam = new(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        private static readonly object _obj = new
        {
            prop1 = 1.1,
            prop2 = "nested string value",
            prop3 = new { child1 = "test", child2 = 42 }
        };
        private static readonly Dictionary<int, string> _keyValuePairs = new()
        {
            {1, "value 1"}, {2, "value 2"}, {3, "value 3"}, {4, "value 4"}, {5, "value 5"},
        };
        private static readonly int[] _intArray = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

        private static readonly object[] _complexParams = [
            _intParam, _stringParam, _doubleParam, _floatParam, _dateTimeParam, _dateTimeOffsetParam, _obj, _keyValuePairs, _intArray
        ];

        [Params(10, 50, 200)]
        public int BatchSize;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var logger = new LoggerConfiguration().CreateLogger();

            var simple = new List<LogEvent>();
            var complex = new List<LogEvent>();

            for (int i = 0; i < 200; i++)
            {
                simple.Add(logger.Create(i % 2 == 0 ? LogEventLevel.Information : LogEventLevel.Debug, $"Simple text message {i}"));
                complex.Add(logger.Create(i % 2 == 0 ? LogEventLevel.Information : LogEventLevel.Debug,
                    "Parametrized {0}; {1}; {2}; {3}; {4}; {5}; {6}; {7}; {8}", propertyValues: _complexParams));
            }

            _simpleEvents = [.. simple];
            _complexEvents = [.. complex];

            var comparer = new LokiLogEventComparer(new LokiSinkConfigurations
            {
                HandleLogLevelAsLabel = true,
                Labels = [new("app", "bench")],
                Url = new Uri("http://localhost"),
            });

            _writer = new LokiMessageWriter(
                new LokiSinkConfigurations
                {
                    HandleLogLevelAsLabel = true,
                    EnrichTraceId = true,
                    EnrichSpanId = true,
                    Labels = [new("app", "bench")],
                    Url = new Uri("http://localhost"),
                },
                comparer,
                new DefaultLokiExceptionFormatter());
        }

        [Benchmark(Baseline = true)]
        public void Serialize_SimpleEvents()
        {
            using var bufferWriter = new PooledByteBufferWriter(1024 * 4);
            using var jsonWriter = new Utf8JsonWriter(bufferWriter);
            _writer.Write(jsonWriter, _simpleEvents.AsSpan(0, BatchSize).ToArray());
            jsonWriter.Flush();
        }

        [Benchmark]
        public void Serialize_ComplexEvents()
        {
            using var bufferWriter = new PooledByteBufferWriter(1024 * 4);
            using var jsonWriter = new Utf8JsonWriter(bufferWriter);
            _writer.Write(jsonWriter, _complexEvents.AsSpan(0, BatchSize).ToArray());
            jsonWriter.Flush();
        }
    }
}
