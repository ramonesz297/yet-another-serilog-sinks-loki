# Serilog.Sinks.Loki.YetAnother


## Installation

```
dotnet add package Serilog.Sinks.Loki.YetAnother
```

## Features

- netstandard2.0 support (sinse v3.0.0)
- Batching support
- trace and span id support
- customizable Exception formatting
- Serilog.Settings.Configuration integration



## Example

```csharp

    Log.Logger = new LoggerConfiguration()
          .WriteTo.Loki(new LokiSinkConfigurations()
          {
              Credentials = new LokiCredentials("< login here >", "< password here >"),
              Url = new Uri("< uri to loki server here >"),
              HandleLogLevelAsLabel = true, // adds Serilog.Events.LogEvent.Level as label (default is true)
              PropertiesAsLabels = ["userId"], // adds Serilog.Events.LogEvent.Properties as labels (default is empty)
              Labels = //global labels, will be added to each loki log message (default is empty)
              [
                  new LokiLabel("app", "loki"),
              ]
          },
          batchSizeLimit: 1000, //The maximum number of events to include in a single batch (default is 1000)
          period: TimeSpan.FromMilliseconds(2000), // period between sending batches to loki (default is 2000ms)
          queueLimit: 100000, //Maximum number of events to hold in the sink's internal queue, or null for an unbounded queue
          httpClient: null) // custom HttpClient instance, (can be used to set proxy, compression etc)
          .CreateLogger();

```

## Using Serilog.Settings.Configurations


#### appsettings.json
```json

{
  "Serilog": {
    "Using": [ "Serilog.Sinks.Loki" ],
    "WriteTo": [
      {
        "Name": "Loki",
        "Args": {
          "configurations": {
            "Url": "https://logs-prod42.grafana.net",
            "Labels": [
              {
                "key": "app",
                "value": "myapp"
              }
            ],
            "Credentials": {
              "Password": "password",
              "Username": "username"
            },
            "PropertiesAsLabels": [
              "app",
              "environment"
            ],
            "HandleLogLevelAsLabel": false,
            "EnrichTraceId": true, //enrich log with TraceId 
            "EnrichSpanId": true //enrich log with SpanId 
          },
          "batchSizeLimit": 999
        }
      }
    ]
  }
}

```

#### Program.cs
```csharp

using Microsoft.Extensions.Configuration;
using Serilog;

var builder = new ConfigurationBuilder()
    .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder)
    .CreateLogger();


```

## Benchmarks

```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4602/23H2/2023Update/SunValley3)
Intel Core i5-14600K, 1 CPU, 20 logical and 14 physical cores
  [Host]                        : .NET Framework 4.8.1 (4.8.9282.0), X64 RyuJIT VectorSize=256
  ShortRun-.NET 9.0             : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  ShortRun-.NET Framework 4.8.1 : .NET Framework 4.8.1 (4.8.9282.0), X64 RyuJIT VectorSize=256

IterationCount=3  LaunchCount=1  WarmupCount=3

```

| Method                     | Runtime              | Count | Mean           | Ratio | Gen0       | Gen1       | Gen2       | Allocated    | Alloc Ratio |
|--------------------------- |--------------------- |------ |---------------:|------:|-----------:|-----------:|-----------:|-------------:|------------:|
| Serilog_Sinks_Grafana_Loki | .NET 9.0  | 50    | 2,053,614.2 us | 0.995 |          - |          - |          - |  16774.91 KB |       19.11 |
| Empty                      | .NET 9.0  | 50    |       246.1 us | 0.000 |    55.1758 |     1.7090 |          - |    678.69 KB |        0.77 |
| YetAnotherLoki             | .NET 9.0  | 50    | 2,063,304.4 us | 1.000 |          - |          - |          - |    877.98 KB |        1.00 |
|                            |           |       |                |       |            |            |            |              |             |
| Serilog_Sinks_Grafana_Loki | .NF 4.8.1 | 50    |    22,956.5 us |  1.96 |  2750.0000 |  1750.0000 |   781.2500 |  21138.92 KB |        4.47 |
| Empty                      | .NF 4.8.1 | 50    |       587.0 us |  0.05 |   110.3516 |     3.9063 |     0.9766 |     682.4 KB |        0.14 |
| YetAnotherLoki             | .NF 4.8.1 | 50    |    11,752.9 us |  1.00 |   718.7500 |   640.6250 |   515.6250 |   4728.41 KB |        1.00 |
|                            |           |       |                |       |            |            |            |              |             |
| Serilog_Sinks_Grafana_Loki | .NET 9.0  | 100   | 2,059,045.6 us | 1.009 |  1000.0000 |          - |          - |  33492.23 KB |       19.73 |
| Empty                      | .NET 9.0  | 100   |       492.2 us | 0.000 |   109.3750 |          - |          - |   1349.08 KB |        0.79 |
| YetAnotherLoki             | .NET 9.0  | 100   | 2,040,266.7 us | 1.000 |          - |          - |          - |   1697.38 KB |        1.00 |
|                            |           |       |                |       |            |            |            |              |             |
| Serilog_Sinks_Grafana_Loki | .NF 4.8.1 | 100   |    36,404.6 us |  1.43 |  4571.4286 |  1571.4286 |  1000.0000 |  44212.28 KB |        3.83 |
| Empty                      | .NF 4.8.1 | 100   |     1,183.1 us |  0.05 |   218.7500 |     7.8125 |     1.9531 |   1354.86 KB |        0.12 |
| YetAnotherLoki             | .NF 4.8.1 | 100   |    25,843.7 us |  1.02 |  1200.0000 |  1066.6667 |   800.0000 |  11548.38 KB |        1.00 |
|                            |           |       |                |       |            |            |            |              |             |
| Serilog_Sinks_Grafana_Loki | .NET 9.0  | 1000  | 2,296,610.2 us | 1.059 | 18000.0000 | 10000.0000 |  3000.0000 | 350725.77 KB |       21.40 |
| Empty                      | .NET 9.0  | 1000  |     4,877.4 us | 0.002 |  1093.7500 |    31.2500 |          - |  13415.25 KB |        0.82 |
| YetAnotherLoki             | .NET 9.0  | 1000  | 2,169,810.6 us | 1.000 |  1000.0000 |          - |          - |  16389.55 KB |        1.00 |
|                            |           |       |                |       |            |            |            |              |             |
| Serilog_Sinks_Grafana_Loki | .NF 4.8.1 | 1000  |   418,773.4 us |  1.28 | 46000.0000 | 16000.0000 |  8000.0000 | 488816.86 KB |        3.38 |
| Empty                      | .NF 4.8.1 | 1000  |    11,611.0 us |  0.04 |  2187.5000 |    46.8750 |    15.6250 |     13458 KB |        0.09 |
| YetAnotherLoki             | .NF 4.8.1 | 1000  |   327,267.6 us |  1.00 |  4000.0000 |  2000.0000 |  1000.0000 | 144823.27 KB |        1.00 |
|                            |           |       |                |       |            |            |            |              |             |
| Serilog_Sinks_Grafana_Loki | .NET 9.0  | 2000  | 2,511,604.7 us | 1.092 | 37000.0000 | 23000.0000 |  6000.0000 | 701410.65 KB |       21.44 |
| Empty                      | .NET 9.0  | 2000  |     9,679.5 us | 0.004 |  2187.5000 |    46.8750 |          - |  26822.43 KB |        0.82 |
| YetAnotherLoki             | .NET 9.0  | 2000  | 2,299,254.5 us | 1.000 |  2000.0000 |  1000.0000 |          - |  32714.22 KB |        1.00 |
|                            |           |       |                |       |            |            |            |              |             |
| Serilog_Sinks_Grafana_Loki | .NF 4.8.1 | 2000  |   764,830.3 us |  1.21 | 85000.0000 | 28000.0000 | 10000.0000 | 977264.74 KB |        3.39 |
| Empty                      | .NF 4.8.1 | 2000  |    23,820.7 us |  0.04 |  4375.0000 |    62.5000 |    31.2500 |  26903.93 KB |        0.09 |
| YetAnotherLoki             | .NF 4.8.1 | 2000  |   636,940.6 us |  1.01 | 11000.0000 |  7000.0000 |  5000.0000 | 288599.45 KB |        1.00 |

## TODO
 
 - improve documentation
 - add more examples
 - add more tests

## I know there are already a few Serilog Loki sinks out there, why another one?

Widly used loki sinks are good and work as expected. But if app produces a lot of logs, in the future this might allocate a lot of objects.

This sink is designed to use less memory and allocate less objects. 

The main idea is to use `Utf8JsonWriter` to write logs directly to the stream.

Moreover, to decrease GC pressure, and memory allocation, it uses custom TextWriter, that writes message directly to the ut8 stream,
instead of usual way - StringWriter. 

This approach allows to decrease number of allocations and its avg size, and decrease number of GC cycles as well.

As the result, you can send more logs with less drawbacks.


## Inspiration and Credits

- [Serilog.Sinks.Grafana.Loki](https://github.com/serilog-contrib/serilog-sinks-grafana-loki)