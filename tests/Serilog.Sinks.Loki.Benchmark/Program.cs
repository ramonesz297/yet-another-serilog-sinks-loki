// This file is part of the project licensed under the MIT License.
// See the LICENSE file in the project root for more information.


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

