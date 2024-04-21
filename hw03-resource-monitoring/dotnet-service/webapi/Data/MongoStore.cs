using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace webapi.Data;

public class AnalyticsEventMongoDocument : IAnalyticsEvent
{
    public ObjectId Id { get; set; }
    public string Event { get; set; }
    public string UserId { get; set; }
    public IReadOnlyDictionary<string, string> Properties { get; set; }
    public DateTime Timestamp { get; set; }
}

public class AnalyticsEventMongoStore(IMongoDatabase database) : IAnalyticsEventStore
{
    private readonly IMongoCollection<AnalyticsEventMongoDocument> _collection =
        database.GetCollection<AnalyticsEventMongoDocument>("analytics.events");

    public string Name => "MongoDB";

    public async Task InsertAsync(IAnalyticsEvent analyticsEvent)
    {
        ArgumentNullException.ThrowIfNull(analyticsEvent);
        
        var document = new AnalyticsEventMongoDocument
        {
            Event = analyticsEvent.Event,
            UserId = analyticsEvent.UserId,
            Properties = analyticsEvent.Properties,
            Timestamp = analyticsEvent.Timestamp
        };

        await _collection.InsertOneAsync(document);
    }

    public async Task<IReadOnlyCollection<IAnalyticsEvent>> GetLatestAsync(DateTime dateFrom, DateTime dateTo)
    {
        var fb = Builders<AnalyticsEventMongoDocument>.Filter;
        var filter = fb.Gte(x => x.Timestamp, dateFrom) & fb.Lte(x => x.Timestamp, dateTo);

        var documents = await _collection.Find(filter).ToListAsync();

        return documents;
    }
}

public class MongoStoreInitializer(IMongoDatabase database) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var collection = database.GetCollection<AnalyticsEventMongoDocument>("analytics.events");

        var indexBuilder = Builders<AnalyticsEventMongoDocument>.IndexKeys;

        CreateIndexModel<AnalyticsEventMongoDocument>[] indexes =
        [
            new CreateIndexModel<AnalyticsEventMongoDocument>(indexBuilder.Descending(n => n.Timestamp))
        ];

        await collection.Indexes.CreateManyAsync(indexes, cancellationToken: cancellationToken);

        BsonClassMap[] mappings =
        [
            new BsonClassMap<AnalyticsEventMongoDocument>(
                builder =>
                {
                    builder.AutoMap();
                    builder.MapIdProperty(x => x.Id);
                })
        ];

        foreach (var classMap in mappings)
        {
            if (!BsonClassMap.IsClassMapRegistered(classMap.ClassType))
            {
                BsonClassMap.RegisterClassMap(classMap);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddMongoStore(this IServiceCollection services, IConfiguration configuration)
    {
        var mongoConnection = configuration.GetValue("MongoConnection", "mongodb://localhost:27017");
        var mongoDatabase = configuration.GetValue("MongoDatabase", "Analytics");

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnection));
        services.AddSingleton(provider =>
            provider.GetRequiredService<IMongoClient>().GetDatabase(mongoDatabase));
        
        services.AddSingleton<IAnalyticsEventStore, AnalyticsEventMongoStore>();
        
        services.AddHostedService<MongoStoreInitializer>();

        return services;
    }
}
