# Serilog.Sinks.Loki.YetAnother


## I know there are already a few Serilog Loki sinks out there, why another one?

Widly used loki sinks are good and work as expected. But if app produces a lot of logs, in the future this might allocate a lot of objects.

This sink is designed to use less memory and allocate less objects. 

The main idea is to use `Utf8JsonWriter` to write logs directly to the stream.

Moreover, to decrease GC pressure, and memory allocation, it uses custom TextWriter, that writes message directly to the ut8 stream,
instead of usual way - StringWriter. 

This approach allows to decrease number of allocations and its avg size, and decrease number of GC cycles as well.

As the result, you can send more logs with less drawbacks.


### Example

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

### Using Serilog.Settings.Configurations


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
            "EnrichTraceId": true,
            "EnrichSpanId": true
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

### Benchmarks

```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3007/23H2/2023Update/SunValley3)
Intel Core i5-14600K, 1 CPU, 20 logical and 14 physical cores
.NET SDK 8.0.101
  [Host]   : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2
  ShortRun : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2

Job=ShortRun  InvocationCount=1  IterationCount=3  
LaunchCount=1  UnrollFactor=1  WarmupCount=3  

```
| Method    | Mean     | Error     | StdDev  | Ratio | RatioSD | Completed Work Items | Lock Contentions | Allocated | Alloc Ratio |
|---------- |---------:|----------:|--------:|------:|--------:|---------------------:|-----------------:|----------:|------------:|
| Default   | 150.9 ms | 138.60 ms | 7.60 ms |  1.00 |    0.00 |               6.0000 |           1.0000 |   5.23 MB |        1.00 |
| Optimized | 157.4 ms |  96.08 ms | 5.27 ms |  1.05 |    0.08 |              11.0000 |           1.0000 |   1.18 MB |        0.23 |



Allocations are decreased by 4.4 times compared to `Serilog.Sinks.Grafana.Loki` package, that is great!

However, this banchmarking does not properly handle these cases, so execution time results may not be accurate.



### Inspiration and Credits

- [Serilog.Sinks.Grafana.Loki](https://github.com/serilog-contrib/serilog-sinks-grafana-loki)