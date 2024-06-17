using System.Diagnostics;
using C5;

namespace App;

public class BenchmarkTreeManual
{
    const int MinValue = 0;
    const int MaxValue = int.MaxValue;

    const int Iterations = 1000;
    const int Items = 20_001;
    const int BatchSize = 400;
    
    private readonly int[] _values = new int[Items];

    public BenchmarkTreeManual()
    {
        for (var i = 0; i < Items; i++)
        {
            _values[i] = Random.Shared.Next(MinValue, MaxValue);
        }
    }
    
    public void Run()
    {
        TestAdd();
        // TestRemove();
    }

    void TestAdd()
    {
        const int writeEvery = 100;
        var results = new long[Iterations, Items];
        var stopWatch = new Stopwatch();
        
        for (var i = 0; i < Iterations; i++)
        {
            var tree = new TreeBag<int>();

            for(var j = 0; j < Items; j++)
            {
                stopWatch.Restart();
                
                tree.Add(_values[j]);
                
                stopWatch.Stop();

                results[i, j] = stopWatch.ElapsedTicks;
            }
        }

        for (var i = 0; i < Items; i++)
        {
            long sum = 0;
            
            for (var j = 0; j < Iterations; j++)
            {
                sum += results[j, i];
            }

            if (i % writeEvery == 0)
            {
                Console.WriteLine($"{i + 1}\t{sum / Iterations}");
            }
        }
    }
    
    void TestRemove()
    {
        const int removeCount = 5;

        var resultsDictionary = new Dictionary<int, long>();
        var sw = new Stopwatch();
        var tree = new AvlTree<int>();

        for (var j = 0; j < Items; j++)
        {
            tree.Add(_values[j]);

            if (j != 0 && j % BatchSize == 0)
            {
                var iterationResults = new long[removeCount];

                for (var i = 0; i < removeCount; i++)
                {
                    var value = _values[Random.Shared.Next(0, Items)];

                    sw.Restart();

                    tree.Remove(value);

                    sw.Stop();

                    iterationResults[i] = sw.ElapsedTicks;
                }

                resultsDictionary[j] = iterationResults.Sum() / iterationResults.Length;
            }
        }

        foreach ((var i, var elapsedTicks) in resultsDictionary) Console.WriteLine($"{i}\t{elapsedTicks}");
    }
    
    void TestContains()
    {
        var resultsDictionary = new Dictionary<int, long>();
        var sw = new Stopwatch();
        var tree = new AvlTree<int>();

        for (var j = 0; j < Items; j++)
        {
            tree.Add(_values[j]);

            if (j != 0 && j % BatchSize == 0)
            {
                var iterationResults = new long[Iterations];

                for (var i = 0; i < Iterations; i++)
                {
                    var value = _values[Random.Shared.Next(0, Items)];

                    sw.Restart();

                    tree.Contains(value);

                    sw.Stop();

                    iterationResults[i] = sw.ElapsedTicks;
                }

                resultsDictionary[j] = iterationResults.Sum() / iterationResults.Length;
            }
        }

        foreach ((var i, var elapsedTicks) in resultsDictionary) Console.WriteLine($"{i}\t{elapsedTicks}");
    }
}