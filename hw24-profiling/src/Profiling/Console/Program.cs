using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using ClassLibrary;

var config = DefaultConfig.Instance
    .AddDiagnoser(MemoryDiagnoser.Default)
    .AddColumn(StatisticColumn.OperationsPerSecond)
    .HideColumns(StatisticColumn.StdDev, StatisticColumn.Median, StatisticColumn.Error)
    .WithOption(ConfigOptions.DisableOptimizationsValidator, true);

BenchmarkRunner.Run<BenchmarkAll>(config);

public class BenchmarkAll : BenchmarkBase
{
    [IterationSetup]
    public void SetupIteration() => RestoreTree();
    
    [Benchmark]
    public void Add() => Tree.Add(RandomNumber());

    [Benchmark]
    public bool Contains() => Tree.Contains(Numbers[Random.Shared.Next(0, N)]);

    [Benchmark]
    public void Remove() => Tree.Remove(Numbers[Random.Shared.Next(0, N)]);
}

public class BenchmarkAdd : BenchmarkBase
{
    [IterationSetup]
    public void SetupIteration()
    {
        RestoreTree();
    }
    
    [Benchmark]
    public void Add()
    {
        Tree.Add(RandomNumber());
    }
}

public class BenchmarkContains : BenchmarkBase
{
    [Benchmark]
    public bool Contains()
    {
        return Tree.Contains(RandomNumber());
    }
}

public class BenchmarkRemove : BenchmarkBase
{
    [IterationSetup]
    public void SetupIteration()
    {
        RestoreTree();
    }
    
    [Benchmark]
    public void Remove()
    {
        Tree.Remove(Numbers[Random.Shared.Next(0, N)]);
    }
}

public class BenchmarkBase
{
    // [Params(4000, 8000, 12000, 16000, 20000, 24000, 28000, 32000)]
    [Params(4000, 8000, 12000, 16000, 20000)]
    // [Params(4000, 8000, 12000)]
    public int N;
    
    [GlobalSetup]
    public void Setup()
    {
        Numbers = new List<int>(N);
        
        for (var i = 0; i < N; i++)
        {
            var number = RandomNumber();
            
            Numbers.Add(number);
        }
    }

    protected AvlTree<int> Tree;
    protected List<int> Numbers { get; private set; }

    protected int RandomNumber() => Random.Shared.Next(0, N * 5);

    protected void RestoreTree()
    {
        Tree = new AvlTree<int>();
        
        for (var i = 0; i < N; i++)
        {
            Tree.Add(Numbers[i]);
        }
    }
}
