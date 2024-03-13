var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapPost("/analytics/events", () =>
{
    return new { Message = "Event received" };
});

app.MapGet("/analytics/events/summary", () =>
{
    return new { Message = "Event summary" };
});

await app.RunAsync();

