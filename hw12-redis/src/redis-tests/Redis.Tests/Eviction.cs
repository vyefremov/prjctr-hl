using StackExchange.Redis;
using Xunit.Abstractions;

namespace Redis.Tests;

public abstract class Eviction
{
    private const string ConnectionString = "localhost,allowAdmin=true,serviceName=mymaster";

    protected Eviction(ITestOutputHelper logger)
    {
        Logger = logger;
        RedisConnection = ConnectionMultiplexer.Connect(ConnectionString);
        RedisDatabase = RedisConnection.GetDatabase();
    }
    
    protected static Random Random { get; } = new();
    protected ITestOutputHelper Logger { get; }
    protected ConnectionMultiplexer RedisConnection { get; }
    protected IDatabase RedisDatabase { get; }
}