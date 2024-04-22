using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Xunit;
using Xunit.Abstractions;

namespace Redis.Tests;

public class EvictionCacheStampede(ITestOutputHelper logger) : Eviction(logger)
{
    private const int Concurrency = 100;
    private const int Operations = 10;
    private const int KeysCount = 10;
    private static readonly TimeSpan TestDuration = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan DatabaseResponseTime = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan DatabaseResponseTimeIncrement = TimeSpan.FromMilliseconds(50);
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromSeconds(4);
    private static readonly TimeSpan ProbabilisticBound = TimeSpan.FromSeconds(2);

    private volatile int _operationsCount;
    private volatile int _databaseOperationsCount;
    private volatile int _probabilisticHistCount;
    
    [Fact]
    public void TestProbabilityMath()
    {
        var expiry = ProbabilisticBound;
        
        while (expiry > TimeSpan.Zero)
        {
            var p = GetProbability(expiry);

            Logger.WriteLine($"{expiry}: {p:P1}");
            
            expiry = expiry.Add(TimeSpan.FromMilliseconds(-100));
        }
    }
    
    [Theory]
    [InlineData(nameof(GetAsync))]
    [InlineData(nameof(GetWithProbabilisticAsync))]
    [InlineData(nameof(GetWithProbabilisticAndLockAsync))]
    public async Task Run(string algorithm)
    {
        var cancellationTokenSource = new CancellationTokenSource(TestDuration);
        var cancellationToken = cancellationTokenSource.Token;

        var keys = Enumerable.Range(0, KeysCount).Select(_ => Guid.NewGuid().ToString()).ToList();

        var sw = Stopwatch.StartNew();
        
        while (!cancellationToken.IsCancellationRequested)
        {
            await Parallel.ForEachAsync(Enumerable.Range(0, Concurrency),
                new ParallelOptions { MaxDegreeOfParallelism = Concurrency },
                async (_, _) =>
                {
                    for (int i = 0; i < Operations; i++)
                    {
                        var key = keys[Random.Next(0, KeysCount)];

                        switch (algorithm)
                        {
                            case nameof(GetAsync):
                                await GetAsync(key);
                                break;
                            case nameof(GetWithProbabilisticAsync):
                                await GetWithProbabilisticAsync(key);
                                break;
                            case nameof(GetWithProbabilisticAndLockAsync):
                                await GetWithProbabilisticAndLockAsync(key);
                                break;
                            default:
                                throw new InvalidOperationException("Invalid algorithm.");
                        }
                        
                        Interlocked.Increment(ref _operationsCount);
                    }
                });
        }
        
        sw.Stop();

        Logger.WriteLine($"Algorithm: {algorithm}");
        Logger.WriteLine($"Operation count: {_operationsCount}");
        Logger.WriteLine($" {sw.Elapsed} elapsed");
        Logger.WriteLine($" {_operationsCount / sw.Elapsed.TotalSeconds:N0} ops/s");
        Logger.WriteLine($"Database operation count: {_databaseOperationsCount} {_databaseOperationsCount * 1.0 / _operationsCount:P4}");
        Logger.WriteLine($"Probabilistic hit count: {_probabilisticHistCount} {_probabilisticHistCount * 1.0 / _operationsCount:P4}");
    }
    
    private async Task GetAsync(string key)
    {
        var result = await RedisDatabase.StringGetWithExpiryAsync(key);

        if (!result.Value.HasValue)
        {
            var queriedValue = await QueryDatabase(key);

            await RedisDatabase.StringSetAsync(key, queriedValue, DefaultExpiration);
        }
    }
    
    private async Task GetWithProbabilisticAsync(string key)
    {
        var result = await RedisDatabase.StringGetWithExpiryAsync(key);

        var probability = result.Expiry.HasValue ? GetProbability(result.Expiry.Value) : 0;

        var probabilisticHit = probability > 0 && Random.NextDouble() < probability;
        
        if (!result.Value.HasValue || probabilisticHit)
        {
            if (probabilisticHit)
            {
                Interlocked.Increment(ref _probabilisticHistCount);
            }
            
            var queriedValue = await QueryDatabase(key);

            await RedisDatabase.StringSetAsync(key, queriedValue, DefaultExpiration);
        }
    }
    
    private async Task GetWithProbabilisticAndLockAsync(string key)
    {
        string lockKey = $"{key}:lock";
        
        var result = await RedisDatabase.StringGetWithExpiryAsync(key);

        var probability = result.Expiry.HasValue ? GetProbability(result.Expiry.Value) : 0;

        var probabilisticHit = probability > 0 && Random.NextDouble() < probability;
        
        if (!result.Value.HasValue || probabilisticHit)
        {
            while (await RedisDatabase.KeyExistsAsync(lockKey))
            {
                await Task.Delay(TimeSpan.FromMicroseconds(20));
            }

            if (probabilisticHit)
            {
                Interlocked.Increment(ref _probabilisticHistCount);
            }

            await RedisDatabase.StringSetAsync(lockKey, "1", TimeSpan.FromSeconds(2));
            
            var queriedValue = await QueryDatabase(key);

            await RedisDatabase.StringSetAsync(key, queriedValue, DefaultExpiration);
            await RedisDatabase.KeyDeleteAsync(lockKey);
        }
    }
    
    private static readonly double ProbabilisticBoundMs = ProbabilisticBound.TotalMilliseconds;
    private static readonly double ProbabilisticBoundFactor = 1 * ProbabilisticBound.TotalMilliseconds; // 1 is exp factor
    private static readonly double EMin = Math.Exp(-ProbabilisticBoundMs / ProbabilisticBoundFactor);
    private static readonly double EMax = Math.Exp(-1 / ProbabilisticBoundFactor);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double GetProbability(TimeSpan expiry)
    {
        if (expiry >= ProbabilisticBound)
        {
            return 0;
        }
        
        var e = Math.Exp(-expiry.TotalMilliseconds / ProbabilisticBoundFactor);

        return (e - EMin) / (EMax - EMin); // Scaled from 0 to 1
    }
    
    private readonly ConcurrentDictionary<string, int> _databaseActiveReads = new();

    private async Task<string> QueryDatabase(string key)
    {
        Interlocked.Increment(ref _databaseOperationsCount);
        
        var activeReads = _databaseActiveReads.AddOrUpdate(key, 1, (_, value) => value + 1);

        var activeReadsDelay = activeReads > 1 ? DatabaseResponseTimeIncrement * (activeReads - 1) : TimeSpan.Zero;

        var databaseResponseTime = DatabaseResponseTime + activeReadsDelay;
        
        await Task.Delay(databaseResponseTime);
        
        _databaseActiveReads.AddOrUpdate(key, 0, (_, value) => value - 1);

        return Guid.NewGuid().ToString();
    }
}
