using Serilog.Sinks.Grafana.Loki;
using GrafanaLokiLabel = Serilog.Sinks.Grafana.Loki.LokiLabel;

namespace Serilog.Sinks.Loki.Benchmark
{
    public static class LoggerConfigurationFactory
    {
        public static LoggerConfiguration Default(string uri)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.GrafanaLoki(uri, [new GrafanaLokiLabel()
                {
                    Key = "app",
                    Value = "sink1"
                }], credentials: new()
                {
                    Login = "login",
                    Password = "pass"
                }, propertiesAsLabels: ["level"], period: TimeSpan.FromMilliseconds(500));
        }

        public static LoggerConfiguration Optimized(string uri)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Loki(new LokiSinkConfigurations()
                {
                    Credentials = new LokiCredentials("login", "pass"),
                    Url = new Uri(uri),
                    HandleLogLevelAsLabel = true,
                    Labels =
                    [
                        new LokiLabel("app", "sinks2"),
                    ],
                }, period: TimeSpan.FromMilliseconds(500));
        }
    }


}
