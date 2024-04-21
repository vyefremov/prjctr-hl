using System.Diagnostics;
using StackExchange.Redis;
using Turbocharged.Beanstalk;
using Xunit;
using Xunit.Abstractions;

namespace Benchmark;

public class Benchmark(ITestOutputHelper logger)
{
    [Theory]
    [InlineData(100_000)]
    public async Task Beanstalkd(int count)
    {
        const string connectionString = "localhost:11300";
        const string tube = "commands";
        
        // Create a producer
        IProducer producer = await BeanstalkConnection.ConnectProducerAsync(connectionString);
        await producer.UseAsync(tube);
        
        // Create a consumer
        IConsumer consumer = await BeanstalkConnection.ConnectConsumerAsync(connectionString);
        await consumer.WatchAsync(tube);
        
        var producerStopwatch = Stopwatch.StartNew();

        // Produce jobs
        for (int i = 0; i < count; i++)
        {
            byte[] job = Guid.NewGuid().ToByteArray();

            await producer.PutAsync(job, priority: 0, delay: TimeSpan.Zero, timeToRun: TimeSpan.FromSeconds(1));
        }
        
        producerStopwatch.Stop();
        
        var consumerStopwatch = Stopwatch.StartNew();

        // Consume jobs
        for (int i = 0; i < count; i++)
        {
            var job = await consumer.ReserveAsync(TimeSpan.FromSeconds(5));
                
            await consumer.DeleteAsync(job.Id);
        }
        
        consumerStopwatch.Stop();

        LogResults("Beanstalkd", producerStopwatch.ElapsedMilliseconds, consumerStopwatch.ElapsedMilliseconds, count);
    }

    [Theory]
    // [InlineData("localhost:6379", "No persistence", 5_000)]
    // [InlineData("localhost:6379", "No persistence", 10_000)]
    // [InlineData("localhost:6379", "No persistence", 100_000)]
    // [InlineData("localhost:6380", "AOF", 5_000)]
    // [InlineData("localhost:6380", "AOF", 10_000)]
    // [InlineData("localhost:6380", "AOF", 100_000)]
    // [InlineData("localhost:6381", "RDB", 5_000)]
    // [InlineData("localhost:6381", "RDB", 10_000)]
    [InlineData("localhost:6381", "RDB", 100_000)]
    public async Task Redis_PubSub(string connectionString, string instanceName, int count)
    {
        const string channel = "commands";

        ConnectionMultiplexer redis = await ConnectionMultiplexer.ConnectAsync(connectionString);
        ISubscriber sub = redis.GetSubscriber();

        DateTime? firstMessageTime = null;
        DateTime? lastMessageTime = null;

        var subscription = await sub.SubscribeAsync(channel);

        subscription.OnMessage(async channelMessage =>
        {
            firstMessageTime ??= DateTime.Now;
            lastMessageTime = DateTime.Now;
            
            await Task.CompletedTask;
        });
        
        var producerStopwatch = Stopwatch.StartNew();

        for (int i = 0; i < count; i++)
        {
            await sub.PublishAsync(channel, Guid.NewGuid().ToString());
        }
        
        producerStopwatch.Stop();

        await Task.Delay(TimeSpan.FromSeconds(1));
        
        // wait for all messages to be received
        while (DateTime.Now - lastMessageTime < TimeSpan.FromSeconds(1))
        {
            await Task.Delay(500);
        }

        await sub.UnsubscribeAllAsync();
        
        LogResults($"Redis PubSub ({instanceName})", producerStopwatch.ElapsedMilliseconds, (lastMessageTime - firstMessageTime)!.Value.TotalMilliseconds, count);
    }

    [Theory]
    // [InlineData("localhost:6379", "No persistence", 5_000)]
    // [InlineData("localhost:6379", "No persistence", 10_000)]
    // [InlineData("localhost:6379", "No persistence", 100_000)]
    // [InlineData("localhost:6380", "AOF", 5_000)]
    // [InlineData("localhost:6380", "AOF", 10_000)]
    // [InlineData("localhost:6380", "AOF", 100_000)]
    // [InlineData("localhost:6381", "RDB", 5_000)]
    // [InlineData("localhost:6381", "RDB", 10_000)]
    [InlineData("localhost:6381", "RDB", 100_000)]
    public async Task Redis_Queue(string connectionString, string instanceName, int count)
    {
        const string channel = "commands";

        ConnectionMultiplexer redis = await ConnectionMultiplexer.ConnectAsync(connectionString);
        
        IDatabase db = redis.GetDatabase();
        
        var producerStopwatch = Stopwatch.StartNew();

        for (int i = 0; i < count; i++)
        {
            await db.ListRightPushAsync(channel, i.ToString());
        }
        
        producerStopwatch.Stop();
        
        var consumerStopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < count; i++)
        {
            await db.ListLeftPopAsync(channel);
        }
        
        consumerStopwatch.Stop();

        LogResults($"Redis Queue ({instanceName})", producerStopwatch.ElapsedMilliseconds, consumerStopwatch.ElapsedMilliseconds, count);
    }
    
    private void LogResults(string context, double producerElapsedMilliseconds, double consumerElapsedMilliseconds, int count)
    {
        logger.WriteLine($"[{context}] Items: {count}");
        logger.WriteLine($"[{context}] Producer: {producerElapsedMilliseconds:N0} ms; {count / producerElapsedMilliseconds * 1000:N0} items/s");
        logger.WriteLine($"[{context}] Consumer: {consumerElapsedMilliseconds:N0} ms; {count / consumerElapsedMilliseconds * 1000:N0} items/s");
    }
}