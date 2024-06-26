using Prometheus;
using webapi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddSingleton<IAnalyticsEventSummaryBuilder, AnalyticsEventSummaryBuilder>()
    .AddMongoStore(builder.Configuration, builder.Environment);

var app = builder.Build();

app.MapGet("api/v1/version", (IConfiguration c) => Results.Ok(c.GetValue("ImageVersion", "Unknown")));

app.MapPost("api/v1/analytics/events", async (AnalyticsEvent request, IEnumerable<IAnalyticsEventStore> stores) =>
{
    await Task.WhenAll(stores.Select(store => store.InsertAsync(request)));

    return Results.Accepted();
});

app.MapGet("api/v1/analytics/events/summary",
    async (DateTime? from,
        DateTime? to,
        IEnumerable<IAnalyticsEventStore> stores,
        IAnalyticsEventSummaryBuilder summaryBuilder) =>
{
    var results = new Dictionary<string, AnalyticsEventSummary>();

    from ??= DateTime.UtcNow.AddSeconds(-5);
    to ??= DateTime.UtcNow.AddSeconds(1);

    await Parallel.ForEachAsync(stores, async (store, _) =>
    {
        var events = await store.GetLatestAsync(from.Value, to.Value);

        var summary = summaryBuilder.BuildSummary(events);

        results[store.Name] = summary;
    });

    return Results.Ok(results);
});

app.MapMetrics();
app.UseHttpMetrics();

await app.RunAsync();

public partial class Program;
