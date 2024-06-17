using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using C5;

namespace App;

public class BenchmarkTree
{
    public static void Run()
    {
        var config = DefaultConfig.Instance
            .AddDiagnoser(MemoryDiagnoser.Default)
            .AddColumn(StatisticColumn.OperationsPerSecond)
            .HideColumns(StatisticColumn.StdDev, StatisticColumn.Median, StatisticColumn.Error)
            .WithOption(ConfigOptions.DisableOptimizationsValidator, true);

        BenchmarkRunner.Run<BenchmarkAdd>(config);
    }
}

public class BenchmarkAdd : BenchmarkBase
{
    [Benchmark]
    public void Add()
    {
        RestoreTree();
    }
}

public class BenchmarkBase
{
    [Params(10, 500, 1000, 1500, 2000, 2500, 3000, 3500, 4000, 4500, 5000, 5500, 6000, 6500, 7000, 7500, 8000)]
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

        RestoreTree();
    }

    // protected BalancedBinarySearchTreeImported.AvlTreeImported<int> Tree;
    protected TreeBag<int> Tree;
    protected List<int> Numbers { get; private set; }

    protected int RandomNumber() => Random.Shared.Next();

    protected void RestoreTree()
    {
        // Tree = new BalancedBinarySearchTreeImported.AvlTreeImported<int>();
        Tree = new TreeBag<int>();
        
        for (var i = 0; i < N; i++)
        {
            Tree.Add(Numbers[i]);
        }
    }
}
