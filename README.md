# Serilog.Sinks.Loki.YetAnother

A performance-oriented [Serilog](https://serilog.net/) sink that sends log events to [Grafana Loki](https://grafana.com/oss/loki/) using the HTTP push API. Designed to minimize memory allocations and GC pressure, making it a great fit for high-throughput .NET applications.

## Features

- **Low allocation design** — writes JSON directly to a UTF-8 stream via `Utf8JsonWriter` and pooled buffers instead of intermediate strings
- **Broad framework support** — targets `net481`, `netstandard2.0`, `net8.0`, `net9.0`, and `net10.0`
- **Batching with back-pressure** — configurable batch size, period, queue limit, and retry policy
- **Distributed tracing** — optional `TraceId` / `SpanId` enrichment from `LogEvent` context
- **Flexible labeling** — global static labels, property-to-label promotion, and automatic log-level labels
- **Multi-tenant support** — set the `X-Scope-OrgID` header per sink via the `Tenant` property
- **Custom exception formatting** — plug in your own `ILokiExceptionFormatter` implementation
- **Serilog.Settings.Configuration** — full support for `appsettings.json`-based setup

## Installation

```
dotnet add package Serilog.Sinks.Loki.YetAnother
```

Or add a `PackageReference` directly:

```xml
<PackageReference Include="Serilog.Sinks.Loki.YetAnother" Version="*" />
```

## Quick Start

```csharp
using Serilog;
using Serilog.Sinks.Loki;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Loki(new LokiSinkConfigurations
    {
        Url = new Uri("https://loki.example.com"),
        Labels =
        [
            new LokiLabel("app", "my-service"),
            new LokiLabel("environment", "production"),
        ]
    })
    .CreateLogger();

Log.Information("Hello from {App}!", "my-service");
```

## Configuration Reference

### Sink Parameters

These are passed directly to `.WriteTo.Loki(...)`:

| Parameter | Type | Default | Description |
|---|---|---|---|
| `configurations` | `LokiSinkConfigurations` | *required* | Core sink settings (see table below) |
| `batchSizeLimit` | `int` | `1000` | Maximum number of events in a single batch |
| `period` | `TimeSpan` | `2 seconds` | Time between batch flush attempts |
| `queueLimit` | `int` | `100000` | Maximum events held in the internal queue. When full, new events are dropped |
| `eagerlyEmitFirstEvent` | `bool` | `true` | Flush immediately when the first event arrives (useful during debugging) |
| `httpClient` | `HttpClient?` | `null` | Supply your own `HttpClient` for proxy, compression, or custom headers |
| `exceptionFormatter` | `ILokiExceptionFormatter?` | `null` | Custom exception formatter (falls back to the built-in recursive formatter) |
| `retryTimeLimit` | `TimeSpan` | `10 minutes` | How long the sink retries a failed batch before discarding it |

### `LokiSinkConfigurations` Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Url` | `Uri` | *required* | Base URL of the Loki server |
| `Labels` | `LokiLabel[]` | `[]` | Global labels added to every log stream |
| `Credentials` | `LokiCredentials?` | `null` | Basic-auth username and password |
| `PropertiesAsLabels` | `string[]` | `[]` | Log event property names to promote to Loki labels. **Matching is case-sensitive.** |
| `HandleLogLevelAsLabel` | `bool` | `true` | Add the Serilog `Level` as a `level` label |
| `Tenant` | `string?` | `null` | When set, an `X-Scope-OrgID` header is sent with every request |
| `EnrichTraceId` | `bool` | `false` | Include `TraceId` from `LogEvent.TraceId` in the JSON payload |
| `EnrichSpanId` | `bool` | `false` | Include `SpanId` from `LogEvent.SpanId` in the JSON payload |

### Programmatic Setup (all options)

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Loki(new LokiSinkConfigurations
    {
        Url = new Uri("https://loki.example.com"),
        Credentials = new LokiCredentials("username", "password"),
        HandleLogLevelAsLabel = true,
        PropertiesAsLabels = ["userId"],
        Labels =
        [
            new LokiLabel("app", "my-service"),
        ],
        Tenant = "team-a",
        EnrichTraceId = true,
        EnrichSpanId = true,
    },
    batchSizeLimit: 500,
    period: TimeSpan.FromSeconds(2),
    queueLimit: 50_000,
    eagerlyEmitFirstEvent: true,
    retryTimeLimit: TimeSpan.FromMinutes(5))
    .CreateLogger();
```

### Serilog.Settings.Configuration (appsettings.json)

#### appsettings.json

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Loki"],
    "WriteTo": [
      {
        "Name": "Loki",
        "Args": {
          "configurations": {
            "Url": "https://loki.example.com",
            "Labels": [
              { "key": "app", "value": "my-service" }
            ],
            "Credentials": {
              "Username": "username",
              "Password": "password"
            },
            "PropertiesAsLabels": ["app", "environment"],
            "HandleLogLevelAsLabel": true,
            "EnrichTraceId": true,
            "EnrichSpanId": true,
            "Tenant": "team-a"
          },
          "batchSizeLimit": 500
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

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();
```

> A runnable sample project is available under [`samples/SettingsConfigurations`](samples/SettingsConfigurations).

## Advanced Usage

### Custom HttpClient

Supply your own `HttpClient` to configure proxies, compression, timeouts, or custom headers:

```csharp
var httpClient = new HttpClient(new HttpClientHandler
{
    Proxy = new WebProxy("http://proxy.example.com:8080"),
    AutomaticDecompression = DecompressionMethods.GZip,
});
httpClient.Timeout = TimeSpan.FromSeconds(30);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Loki(new LokiSinkConfigurations
    {
        Url = new Uri("https://loki.example.com"),
    },
    httpClient: httpClient)
    .CreateLogger();
```

> **Note:** When you supply your own `HttpClient`, you are responsible for its lifecycle (disposal).

### Custom Exception Formatting

Implement `ILokiExceptionFormatter` to control how exceptions are serialized:

```csharp
public class SimpleExceptionFormatter : ILokiExceptionFormatter
{
    public void Format(Utf8JsonWriter writer, Exception exception)
    {
        writer.WriteStringValue($"{exception.GetType().Name}: {exception.Message}");
    }
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.Loki(new LokiSinkConfigurations
    {
        Url = new Uri("https://loki.example.com"),
    },
    exceptionFormatter: new SimpleExceptionFormatter())
    .CreateLogger();
```

## Label Strategy

Loki indexes labels, not log content. Choosing the right labels is critical for query performance and cluster health.

**Do:**
- Use a small, fixed set of labels (e.g., `app`, `environment`, `level`).
- Promote log properties to labels only when their cardinality is low and bounded.

**Avoid:**
- High-cardinality values as labels (user IDs, request IDs, trace IDs). These should stay in the log message JSON and be queried with LogQL filters.
- Adding many labels — Loki performs best with fewer than ~15 labels per stream.

`PropertiesAsLabels` matching is **case-sensitive** — `"userId"` will only match a property named exactly `userId`.

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| No logs appear in Loki | Incorrect `Url` or network connectivity | Verify the Loki push endpoint is reachable from the application host |
| `401 Unauthorized` | Missing or wrong credentials | Check `Credentials` username/password; verify basic auth is enabled on the Loki gateway |
| Logs are delayed or missing under load | Queue full — events are dropped when `queueLimit` is exceeded | Increase `queueLimit`, decrease `period`, or increase `batchSizeLimit` |
| Retries consuming memory | Default `retryTimeLimit` is 10 minutes | Lower `retryTimeLimit` for high-load scenarios to reduce buffering |
| Labels not appearing | Property name casing mismatch in `PropertiesAsLabels` | Ensure the property name matches exactly (case-sensitive) |
| Multi-tenant header not sent | `Tenant` not set | Set the `Tenant` property on `LokiSinkConfigurations` |

## Why Another Loki Sink?

Widely used Loki sinks work well, but under high log volume they allocate many intermediate objects, increasing GC pressure.

This sink is designed from the ground up for low memory overhead:

- **`Utf8JsonWriter`** writes log events directly to the HTTP request stream — no intermediate `string` or `StringBuilder` allocations.
- **Custom `TextWriter`** renders Serilog message templates straight into the UTF-8 stream, bypassing the usual `StringWriter` path.
- **Pooled buffers** reuse memory across batches, reducing both allocation count and average allocation size.

The result: fewer GC cycles and lower memory footprint, especially at scale.

## Inspiration and Credits

- [Serilog.Sinks.Grafana.Loki](https://github.com/serilog-contrib/serilog-sinks-grafana-loki)

## License

This project is licensed under the [MIT License](LICENSE.txt).