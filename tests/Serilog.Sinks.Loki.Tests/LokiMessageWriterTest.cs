﻿using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.Loki.Internal;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Serilog.Sinks.Loki.Tests
{
    public class LokiMessageWriterTest : IDisposable
    {

        private readonly PooledTextWriterAndByteBufferWriterOwner _pooledTextWriterAndByteBufferWriterOwner = new PooledTextWriterAndByteBufferWriterOwner();

        private readonly ILokiExceptionFormatter _defaultExceptionFormatter = new DefaultLokiExceptionFormatter();

        private readonly MessageTemplateParser _messageTemplateParser = new MessageTemplateParser();
        public LokiMessageWriterTest()
        {
        }

        private static readonly DateTimeOffset _date = new(2021, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private LokiMessageWriter Create(LokiSinkConfigurations? configurations = null)
        {
            configurations ??= new LokiSinkConfigurations();

            LokiLogEventComparer comparer = new LokiLogEventComparer(configurations);

            return new LokiMessageWriter(configurations, _pooledTextWriterAndByteBufferWriterOwner, comparer, _defaultExceptionFormatter);
        }

        [Theory]
        [InlineData(LogEventLevel.Debug)]
        [InlineData(LogEventLevel.Information)]
        [InlineData(LogEventLevel.Verbose)]
        [InlineData(LogEventLevel.Warning)]
        [InlineData(LogEventLevel.Error)]
        [InlineData(LogEventLevel.Fatal)]
        public Task Should_write_simple_log_message_without_parameters(LogEventLevel level)
        {
            using var bufferWriter = _pooledTextWriterAndByteBufferWriterOwner.RentBufferWriter();
            using var jsonWriter = new Utf8JsonWriter(bufferWriter);
            var logWriter = Create();

            var messageTemplate = _messageTemplateParser.Parse("log wihout parameters");

            var log = new LogEvent(_date, level, null, messageTemplate, []);

            logWriter.Write(jsonWriter, [log]);

            jsonWriter.Flush();

            return Verify(Encoding.UTF8.GetString(bufferWriter.WrittenMemory.Span)).UseParameters(level);
        }


        [Fact]
        public Task Should_write_simple_log_message_with_global_lables()
        {
            using var bufferWriter = _pooledTextWriterAndByteBufferWriterOwner.RentBufferWriter();
            using var jsonWriter = new Utf8JsonWriter(bufferWriter);
            var logWriter = Create(new LokiSinkConfigurations()
            {
                Labels = [new LokiLabel("label1", "value1"), new LokiLabel("label2", "value2")],
            });

            var messageTemplate = _messageTemplateParser.Parse("log wihout parameters");

            var log = new LogEvent(_date, LogEventLevel.Information, null, messageTemplate, []);

            logWriter.Write(jsonWriter, [log]);

            jsonWriter.Flush();

            return Verify(Encoding.UTF8.GetString(bufferWriter.WrittenMemory.Span));
        }

        [Theory]
        [InlineData("value1")]
        [InlineData(42)]
        [InlineData(42.43f)]
        [InlineData(42.44d)]
        [InlineData(45L)]
        [InlineData(46UL)]
        [InlineData(47U)]
        [InlineData('l')]
        [InlineData(null)]
        public Task Should_write_log_message_with_props_and_global_lables(object? scalarValue)
        {
            using var bufferWriter = _pooledTextWriterAndByteBufferWriterOwner.RentBufferWriter();
            using var jsonWriter = new Utf8JsonWriter(bufferWriter);
            var logWriter = Create(new LokiSinkConfigurations()
            {
                Labels = [new LokiLabel("label1", "value1"), new LokiLabel("label2", "value2")],
            });

            var messageTemplate = _messageTemplateParser.Parse("log with parameters {Item}");

            LogEventProperty[] properties = [new LogEventProperty("Item", new ScalarValue(scalarValue))];

            var log = new LogEvent(_date, LogEventLevel.Information, null, messageTemplate, properties);

            logWriter.Write(jsonWriter, [log]);

            jsonWriter.Flush();

            var stringValue = scalarValue switch
            {
                null => "null",
                string => "string",
                uint => "uint",
                ulong => "ulong",
                long => "long",
                int => "int",
                char => "char",
                double => "double",
                float => "float",
                _ => throw new ArgumentOutOfRangeException(nameof(scalarValue))
            };
            return Verify(Encoding.UTF8.GetString(bufferWriter.WrittenMemory.Span)).UseParameters(stringValue);
        }


        [Fact]
        public Task Should_write_logs_with_array_parameter()
        {
            using var bufferWriter = _pooledTextWriterAndByteBufferWriterOwner.RentBufferWriter();
            using var jsonWriter = new Utf8JsonWriter(bufferWriter);
            var logWriter = Create(new LokiSinkConfigurations()
            {
                Labels = [new LokiLabel("label1", "value1"), new LokiLabel("label2", "value2")],
            });

            var messageTemplate = _messageTemplateParser.Parse("log with sequence parameters {Item}");

            var arrayParameter = new SequenceValue([new ScalarValue("value1"), new ScalarValue("value2")]);
            LogEventProperty[] properties = [new LogEventProperty("Item", arrayParameter)];

            var log = new LogEvent(_date, LogEventLevel.Information, null, messageTemplate, properties);

            logWriter.Write(jsonWriter, [log]);

            jsonWriter.Flush();

            return Verify(Encoding.UTF8.GetString(bufferWriter.WrittenMemory.Span));
        }

        [Fact]
        public Task Should_write_logs_with_object_parameter()
        {
            using var bufferWriter = _pooledTextWriterAndByteBufferWriterOwner.RentBufferWriter();
            using var jsonWriter = new Utf8JsonWriter(bufferWriter);
            var logWriter = Create(new LokiSinkConfigurations()
            {
                Labels = [new LokiLabel("label1", "value1"), new LokiLabel("label2", "value2")],
            });

            var messageTemplate = _messageTemplateParser.Parse("log with object parameters {Item}");

            var parameter = new StructureValue([
                new LogEventProperty("Property1", new ScalarValue("value1")),
                new LogEventProperty("Property2", new ScalarValue(42))
            ]);
            LogEventProperty[] properties = [new LogEventProperty("Item", parameter)];

            var log = new LogEvent(_date, LogEventLevel.Information, null, messageTemplate, properties);

            logWriter.Write(jsonWriter, [log]);

            jsonWriter.Flush();

            return Verify(Encoding.UTF8.GetString(bufferWriter.WrittenMemory.Span));
        }

        [Fact]
        public Task Should_write_logs_with_dictionary_parameter()
        {
            using var bufferWriter = _pooledTextWriterAndByteBufferWriterOwner.RentBufferWriter();
            using var jsonWriter = new Utf8JsonWriter(bufferWriter);
            var logWriter = Create(new LokiSinkConfigurations()
            {
                Labels = [new LokiLabel("label1", "value1"), new LokiLabel("label2", "value2")],
            });

            var messageTemplate = _messageTemplateParser.Parse("log with dictionary parameters {Item}");

            var parameter = new DictionaryValue(new Dictionary<ScalarValue, LogEventPropertyValue>()
            {
                { new ScalarValue("Key1"), new ScalarValue("value1") },
                { new ScalarValue("Key2"), new ScalarValue(42) }
            });
            LogEventProperty[] properties = [new LogEventProperty("Item", parameter)];

            var log = new LogEvent(_date, LogEventLevel.Information, null, messageTemplate, properties);

            logWriter.Write(jsonWriter, [log]);

            jsonWriter.Flush();

            return Verify(Encoding.UTF8.GetString(bufferWriter.WrittenMemory.Span));
        }

        [Fact]
        public Task Should_write_logs_with_exception()
        {
            using var bufferWriter = _pooledTextWriterAndByteBufferWriterOwner.RentBufferWriter();
            using var jsonWriter = new Utf8JsonWriter(bufferWriter);
            var logWriter = Create(new LokiSinkConfigurations()
            {
                Labels = [new LokiLabel("label1", "value1"), new LokiLabel("label2", "value2")],
            });

            var b = new MessageTemplateParser();

            var messageTemplate = b.Parse("log with error");

            var exception = new InvalidOperationException("Message from exception", new InvalidOperationException("Message from inner exception"));

            var log = new LogEvent(_date, LogEventLevel.Error, exception, messageTemplate, []);

            logWriter.Write(jsonWriter, [log]);

            jsonWriter.Flush();

            return Verify(Encoding.UTF8.GetString(bufferWriter.WrittenMemory.Span));
        }


        [Fact]
        public Task Should_aggregate_logs_by_lables()
        {
            using var bufferWriter = _pooledTextWriterAndByteBufferWriterOwner.RentBufferWriter();
            using var jsonWriter = new Utf8JsonWriter(bufferWriter);
            var logWriter = Create(new LokiSinkConfigurations()
            {
                Labels = [new LokiLabel("label1", "value1")],
            });

            var mtp = _messageTemplateParser;

            var log1_1 = new LogEvent(_date, LogEventLevel.Information, null, mtp.Parse("log #1.1"), []);
            var log1_2 = new LogEvent(_date, LogEventLevel.Information, null, mtp.Parse("log #1.2"), []);
            var log2_1 = new LogEvent(_date, LogEventLevel.Debug, null, mtp.Parse("log #2.1"), []);
            var log2_2 = new LogEvent(_date, LogEventLevel.Debug, null, mtp.Parse("log #2.2"), []);

            logWriter.Write(jsonWriter, [log1_1, log1_2, log2_1, log2_2]);

            jsonWriter.Flush();

            return Verify(Encoding.UTF8.GetString(bufferWriter.WrittenMemory.Span));
        }


        [Fact]
        public Task Should_not_handle_log_level_as_lable()
        {
            using var bufferWriter = _pooledTextWriterAndByteBufferWriterOwner.RentBufferWriter();
            using var jsonWriter = new Utf8JsonWriter(bufferWriter);
            var logWriter = Create(new LokiSinkConfigurations()
            {
                Labels = [new LokiLabel("label1", "value1")],
                HandleLogLevelAsLabel = false
            });

            var mtp = _messageTemplateParser;

            var log1 = new LogEvent(_date, LogEventLevel.Information, null, mtp.Parse("log #1.1"), []);
            var log2 = new LogEvent(_date, LogEventLevel.Debug, null, mtp.Parse("log #1.2"), []);

            logWriter.Write(jsonWriter, [log1, log2]);

            jsonWriter.Flush();

            return Verify(Encoding.UTF8.GetString(bufferWriter.WrittenMemory.Span));
        }

        [Fact]
        public Task Should_add_property_as_lable()
        {
            using var bufferWriter = _pooledTextWriterAndByteBufferWriterOwner.RentBufferWriter();
            using var jsonWriter = new Utf8JsonWriter(bufferWriter);
            var logWriter = Create(new LokiSinkConfigurations()
            {
                Labels = [new LokiLabel("label1", "value1")],
                HandleLogLevelAsLabel = false,
                PropertiesAsLabels = ["userId"]
            });

            var mtp = _messageTemplateParser;

            var userId = new LogEventProperty("userId", new ScalarValue(1));
            var log1 = new LogEvent(_date, LogEventLevel.Information, null, mtp.Parse("log #1.1"), [userId]);

            logWriter.Write(jsonWriter, [log1]);

            jsonWriter.Flush();

            return Verify(Encoding.UTF8.GetString(bufferWriter.WrittenMemory.Span));
        }

        [Fact]
        public Task Should_add_property_out_of_message_template()
        {
            using var bufferWriter = _pooledTextWriterAndByteBufferWriterOwner.RentBufferWriter();
            using var jsonWriter = new Utf8JsonWriter(bufferWriter);
            var logWriter = Create(new LokiSinkConfigurations()
            {
                Labels = [new LokiLabel("label1", "value1")],
                HandleLogLevelAsLabel = false
            });

            var mtp = _messageTemplateParser;

            var userId = new LogEventProperty("userId", new ScalarValue(1));
            var log1 = new LogEvent(_date, LogEventLevel.Information, null, mtp.Parse("log #1.1"), [userId]);

            logWriter.Write(jsonWriter, [log1]);

            jsonWriter.Flush();

            return Verify(Encoding.UTF8.GetString(bufferWriter.WrittenMemory.Span));
        }


        [Fact]
        public Task Should_enrich_span_and_trace_ids()
        {

            using var bufferWriter = _pooledTextWriterAndByteBufferWriterOwner.RentBufferWriter();
            using var jsonWriter = new Utf8JsonWriter(bufferWriter);
            var logWriter = Create(new LokiSinkConfigurations()
            {
                EnrichSpanId = true,
                EnrichTraceId = true
            });

            //hardcoded traceId and spanId
            var traceId = ActivityTraceId.CreateFromString("ed4ad863e4913c6183f28453ebac18e4");
            var activityId = ActivitySpanId.CreateFromString("effd9f82dde27762");

            var messageTemplate = _messageTemplateParser.Parse("log wihout parameters");

            var log = new LogEvent(_date, LogEventLevel.Debug, null, messageTemplate, [], traceId, activityId);

            logWriter.Write(jsonWriter, [log]);
            jsonWriter.Flush();

            return Verify(Encoding.UTF8.GetString(bufferWriter.WrittenMemory.Span));
        }

        public void Dispose()
        {
            _pooledTextWriterAndByteBufferWriterOwner.Dispose();
        }

    }
}