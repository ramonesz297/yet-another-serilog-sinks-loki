// This file is part of the project licensed under the MIT License.
// See the LICENSE file in the project root for more information.

using Serilog.Sinks.Grafana.Loki;
using GrafanaLokiLabel = Serilog.Sinks.Grafana.Loki.LokiLabel;

namespace Serilog.Sinks.Loki.Benchmark
{
    public static class LoggerConfigurationFactory
    {
        public static LoggerConfiguration Serilog_Sinks_Grafana_Loki(string uri)
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
                }, propertiesAsLabels: ["level"]);
        }

        public static LoggerConfiguration YetAnotherLoki(string uri)
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
                });
        }

        public static LoggerConfiguration GrafanaLokiBlackBox(string uri)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.GrafanaLoki(
                    uri,
                    [
                        new GrafanaLokiLabel
                        {
                            Key = "app",
                            Value = "benchmark"
                        }
                    ],
                    credentials: new()
                    {
                        Login = "login",
                        Password = "pass"
                    });
        }

        public static LoggerConfiguration YetAnotherLokiBlackBox(string uri)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Loki(
                    new LokiSinkConfigurations()
                    {
                        Credentials = new LokiCredentials("login", "pass"),
                        Url = new Uri(uri),
                        HandleLogLevelAsLabel = false,
                        Labels =
                        [
                            new LokiLabel("app", "benchmark"),
                        ],
                    });
        }
    }
}
