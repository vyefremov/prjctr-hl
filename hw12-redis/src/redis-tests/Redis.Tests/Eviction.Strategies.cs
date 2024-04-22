using System.Collections.Concurrent;
using StackExchange.Redis;
using Xunit;
using Xunit.Abstractions;

namespace Redis.Tests;

public class EvictionStrategies(ITestOutputHelper logger) : Eviction(logger)
{
    private const string KeyEvictedEventPattern = "__keyevent@*evicted";
    private const int MessagesLimit = 10_000;
    private const double WithExpirationProbability = 0.8;

    [Fact]
    public async Task EvictionTest()
    {
        // Clear all databases
        await RedisConnection.GetServer(RedisConnection.GetEndPoints()[0]).FlushAllDatabasesAsync();

        var stats = new ConcurrentDictionary<string, KeyStats>();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        
        var subscriber = RedisConnection.GetSubscriber();

        await subscriber.SubscribeAsync(
            RedisChannel.Pattern(KeyEvictedEventPattern),
            async (_, key) => await OnKeyEvictedEvent(cancellationTokenSource, key, stats));

        var setTask = Task.Run(async () =>
        {
            for (int i = 0; i < MessagesLimit && !cancellationToken.IsCancellationRequested; i++)
            {
                string key = $"data{i}";

                stats[key] = new KeyStats
                {
                    ExpiresIn = Random.NextDouble() > WithExpirationProbability ? null : TimeSpan.FromSeconds(Random.Next(120, 1200)),
                    LastUsed = DateTime.Now,
                    CountUsed = 1
                };

                var value = string.Join(';', Enumerable.Range(0, 10).Select(_ => Guid.NewGuid().ToString()));

                await RedisDatabase.StringSetAsync(key, value, stats[key].ExpiresIn);

                await Task.Delay(5, cancellationToken); // Simulate additional delay
            }
        }, cancellationToken);

        var getTask = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(50, cancellationToken); // Simulate additional delay

                if (stats.Count < 10)
                {
                    continue;
                }

                var key = stats.Keys.ElementAt(Random.Next(0, stats.Count));

                await RedisDatabase.StringGetAsync(key);

                var keyStats = stats[key];

                keyStats.LastUsed = DateTime.Now;
                keyStats.CountUsed++;
            }
        }, cancellationToken);

        await Task.WhenAll(setTask, getTask).ContinueWith(_ => Task.CompletedTask, TaskContinuationOptions.OnlyOnCanceled);
    }

    private async Task OnKeyEvictedEvent(CancellationTokenSource cancellationTokenSource, RedisValue value,
        ConcurrentDictionary<string, KeyStats> stats)
    {
        await cancellationTokenSource.CancelAsync();

        string key = value;

        var keyStats = stats.GetValueOrDefault(key);

        if (keyStats is null)
        {
            Logger.WriteLine($"Key evicted: {value}; [stats not found]!");
        }

        int lessFrequentlyUsedCount = 0;
        int moreFrequentlyUsedCount = 0;
        int moreRecentlyUsedCount = 0;
        int lessRecentlyUsedCount = 0;
        int expireLaterCount = 0;
        int expireSoonerCount = 0;
        int totalCount = 0;
                    
        foreach (var (k, v) in stats)
        {
            totalCount++;

            if (v.CountUsed < keyStats.CountUsed)
            {
                lessFrequentlyUsedCount++;
            }
            else if (v.CountUsed > keyStats.CountUsed)
            {
                moreFrequentlyUsedCount++;
            }
                    
            if (v.LastUsed > keyStats.LastUsed)
            {
                moreRecentlyUsedCount++;
            }
            else if (v.LastUsed < keyStats.LastUsed)
            {
                lessRecentlyUsedCount++;
            }

            if (keyStats.ExpiresIn.HasValue && v.ExpiresIn.HasValue && v.ExpiresIn.Value > keyStats.ExpiresIn.Value)
            {
                expireLaterCount++;
            }
            else if (keyStats.ExpiresIn is null || (v.ExpiresIn.HasValue && v.ExpiresIn.Value < keyStats.ExpiresIn.Value))
            {
                expireSoonerCount++;
            }
        }

        LogEviction(key, keyStats, lessFrequentlyUsedCount, totalCount, moreFrequentlyUsedCount,
            moreRecentlyUsedCount, lessRecentlyUsedCount, expireLaterCount, expireSoonerCount);

        stats.Remove(key, out _);
    }

    private void LogEviction(string key, KeyStats keyStats, int lessFrequentlyUsedCount, int totalCount,
        int moreFrequentlyUsedCount, int moreRecentlyUsedCount, int lessRecentlyUsedCount, int expireLaterCount,
        int expireSoonerCount)
    {
        lock (this)
        {
            Logger.WriteLine($"Key evicted: {key} [expires in {keyStats.ExpiresIn?.ToString() ?? "<never>"}, used {keyStats.CountUsed} times]");
            Logger.WriteLine($"  % of keys:");
            Logger.WriteLine($"    {lessFrequentlyUsedCount * 1.0 / totalCount:P1} less frequently; {moreFrequentlyUsedCount * 1.0 / totalCount:P1} more frequently");
            Logger.WriteLine($"    {moreRecentlyUsedCount * 1.0 / totalCount:P1} more recently; {lessRecentlyUsedCount * 1.0 / totalCount:P1} less recently");
            Logger.WriteLine($"    {expireLaterCount * 1.0 / totalCount:P1} expire later; {expireSoonerCount * 1.0 / totalCount:P1} expire sooner");
        }
    }

    public class KeyStats
    {
        public TimeSpan? ExpiresIn { get; set; }
        public DateTime LastUsed { get; set; }
        public int CountUsed { get; set; }
    }
}