using BenchmarkDotNet.Running;
using Serilog.Sinks.Grafana.Loki;


namespace Serilog.Sinks.Loki.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("LokiLogin: ");
            Console.WriteLine(Environment.GetEnvironmentVariable("LokiLogin"));

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}

