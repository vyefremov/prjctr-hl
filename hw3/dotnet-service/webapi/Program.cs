using webapi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddMongoStore(builder.Configuration)
    .AddElasticStore(builder.Configuration);

var app = builder.Build();

app.MapPost("/analytics/events", async (AnalyticsEvent request, IEnumerable<IAnalyticsEventStore> stores) =>
{
    await Task.WhenAll(stores.Select(store => store.InsertAsync(request)));

    return Results.Accepted();
});

app.MapGet("/analytics/events/summary", async (DateTime from, DateTime to, IEnumerable<IAnalyticsEventStore> stores) =>
{
    var results = new Dictionary<string, IReadOnlyCollection<IAnalyticsEvent>>();

    await Parallel.ForEachAsync(stores, async (store, token) =>
    {
        var events = await store.GetLatestAsync(from, to);

        results[store.Name] = events;
    });

    return new { Results = results };
});

await app.RunAsync();
