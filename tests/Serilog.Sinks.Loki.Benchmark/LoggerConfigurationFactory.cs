using Serilog.Sinks.Grafana.Loki;
using GrafanaLokiLabel = Serilog.Sinks.Grafana.Loki.LokiLabel;

namespace Serilog.Sinks.Loki.Benchmark
{

    public static class LoggerConfigurationFactory
    {
        public static LoggerConfiguration Default()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.GrafanaLoki(Environment.GetEnvironmentVariable("LokiUrl")!, [new GrafanaLokiLabel()
                {
                    Key = "app",
                    Value = "sink1"
                }], credentials: new()
                {
                    Login = Environment.GetEnvironmentVariable("LokiLogin")!,
                    Password = Environment.GetEnvironmentVariable("LokiPassword")!
                });
        }

        public static LoggerConfiguration Optimized()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Loki(new LokiSinkConfigurations()
                {
                    Credentials = new LokiCredentials(Environment.GetEnvironmentVariable("LokiLogin")!, Environment.GetEnvironmentVariable("LokiPassword")),
                    Url = new Uri(Environment.GetEnvironmentVariable("LokiUrl")!),
                    ExposeLogLevelAsLabel = true,
                    Labels =
                    [
                        new LokiLabel("app", "sinks2"),
                    ],
                });
        }
    }


}
