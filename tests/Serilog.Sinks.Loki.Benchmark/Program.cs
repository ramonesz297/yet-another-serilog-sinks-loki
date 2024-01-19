using BenchmarkDotNet.Running;
using Iced.Intel;
using Microsoft.Diagnostics.Runtime;
using Serilog.Sinks.Loki.Benchmark;

try
{
    int intParam = 1;
    string stringParam = "string";
    double doubleParam = 1.0;
    float floatParam = 1.0f;
    DateTime dateTimeParam = DateTime.Now;
    DateTimeOffset dateTimeOffsetParam = DateTimeOffset.Now;

    var obj = new
    {
        prop1 = 1.1,
        prop2 = "asdasd"
    };

    var dictionary = new Dictionary<string, object>()
    {
        {"2",  intParam},
        {"3",  stringParam},
        {"4",  doubleParam},
        {"5",  floatParam},
    };

    int[] intArray = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

    using (var logger = LoggerConfigurationFactory.Optimized().CreateLogger())
    {
        var log = logger.ForContext<Program>();

        for (int i = 0; i < 10; i++)
        {
            await Task.Delay(50);
            log.Debug("Debug message from optimized logger; {intParam} and {stringParam} and {doubleParam} and {floatParam} and {dateTimeParam} and {dateTimeOffsetParam} {@obj} {dictionary} {intArray}",
                intParam,
                stringParam,
                doubleParam,
                floatParam,
                dateTimeParam,
                dateTimeOffsetParam,
                obj,
                dictionary,
                intArray);

            try
            {
                throw new Exception("test exception", new InvalidOperationException("asdsad"));
            }
            catch (Exception ex)
            {
                log.Error(ex, "test error");
            }
        }
    }


    await Task.Delay(1000);

    Console.WriteLine("End...");
}
catch (Exception)
{

}
