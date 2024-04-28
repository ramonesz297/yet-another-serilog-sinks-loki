using Microsoft.Extensions.Configuration;
using Serilog;

var builder = new ConfigurationBuilder()
    .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder)
    .CreateLogger();
