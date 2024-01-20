using BenchmarkDotNet.Running;


namespace Serilog.Sinks.Loki.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("LokiLogin: ");
            Console.WriteLine(Environment.GetEnvironmentVariable("LokiLogin"));
            BenchmarkRunner.Run<LokiSinkBenchmark>();
        }
    }
}

