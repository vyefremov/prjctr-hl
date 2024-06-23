using Elastic.Clients.Elasticsearch;

namespace webapi.Data;

public class AnalyticsEventElasticDocument : IAnalyticsEvent
{
    public string Id { get; set; }
    public string Event { get; set; }
    public string UserId { get; set; }
    public IReadOnlyDictionary<string, string> Properties { get; set; }
    public DateTime Timestamp { get; set; }
}


public class AnalyticsEventElasticStore(ElasticsearchClient client) : IAnalyticsEventStore
{
    public const string IndexName = "analytics_events";
    public string Name => "Elasticsearch";

    public async Task InsertAsync(IAnalyticsEvent analyticsEvent)
    {
        ArgumentNullException.ThrowIfNull(analyticsEvent);

        var document = new AnalyticsEventElasticDocument
        {
            Id = Guid.NewGuid().ToString(),
            Event = analyticsEvent.Event,
            UserId = analyticsEvent.UserId,
            Properties = analyticsEvent.Properties,
            Timestamp = analyticsEvent.Timestamp
        };

        await client.IndexAsync(document, IndexName);
    }

    public async Task<IReadOnlyCollection<IAnalyticsEvent>> GetLatestAsync(DateTime dateFrom, DateTime dateTo)
    {
        var documents = await client.SearchAsync<AnalyticsEventElasticDocument>(
            s => s
                .Index(IndexName)
                .Query(q => q
                    .Range(r => r
                        .DateRange(dr => dr
                            .Field(f => f.Timestamp)
                            .Gte(dateFrom)
                            .Lte(dateTo)))));
        
        return documents.Hits is { Count: > 0 } ? documents.Documents : [];
    }
}

public class ElasticStoreInitializer(ElasticsearchClient client) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var exists = await client.Indices.ExistsAsync(AnalyticsEventElasticStore.IndexName, cancellationToken);

        if (!exists.Exists)
        {
            await client.Indices.CreateAsync(AnalyticsEventElasticStore.IndexName, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddElasticStore(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.EnvironmentName == "Testing")
        {
            return services;
        }

        var elasticConnection = configuration.GetValue("ElasticsearchConnection", "http://localhost:9200");
        
        services.AddSingleton(_ => new ElasticsearchClient(new Uri(elasticConnection)));
        
        services.AddSingleton<IAnalyticsEventStore, AnalyticsEventElasticStore>();
        
        services.AddHostedService<ElasticStoreInitializer>();

        return services;
    }
}
