using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel((context, serverOptions) =>
{
    serverOptions.Listen(IPAddress.Loopback, 8080, o =>
    {
        o.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
    });
});

builder.Services.AddHealthChecks();
var app = builder.Build();

app.MapHealthChecks("/health").AllowAnonymous();

app.MapPost("loki/api/v1/push", async (HttpContext context, ILogger<Program> logger) =>
{
    var reader = context.Request.BodyReader;
    await reader.CopyToAsync(Stream.Null);
    logger.LogInformation("Received log event");
    return Results.Ok();
}).AllowAnonymous();

app.Run();
