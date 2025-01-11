using BenchmarkDotNet.Running;

namespace Serilog.Sinks.Loki.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}

