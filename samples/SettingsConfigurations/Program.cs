// This file is part of the project licensed under the MIT License.
// See the LICENSE file in the project root for more information.


using Microsoft.Extensions.Configuration;
using Serilog;

var builder = new ConfigurationBuilder()
    .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder)
    .CreateLogger();
