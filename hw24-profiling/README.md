# L24 Homework: Profiling

## Task

1. Implement Balanced Binary Search Tree class and operations of insert/delete/search
2. Profile space usage (Confirm that you see O (n))
3. Profile time consumption (Confirm that you see O (log n))

## Solution

The implementation of the Balanced Binary Search Tree is based on the AVL tree.
The operations are implemented in the [BalancedBinarySearchTree.cs](./src/Profiling/ClassLibrary/BalancedBinarySearchTree.cs).

The profiler of choice is the [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet).
It’s not exactly a profiler; instead, it’s a benchmarking tool. However, it’s much more convenient for profiling the time and space complexity of operations.
For profiling purposes, I'd sure dotTrace to measure execution time; and dotMemory to measure memory usage (based on dump).

Entry point is the [Program](./src/Profiling/Console/Program.cs) class.

### Result table

| Method   | N     |         Mean | Delta % | Allocated per operation |
|----------|-------|-------------:|--------:|------------------------:|
| Add      | 4000  |  29,218.1 ns |         |                    65 B |
| Add      | 8000  |  72,765.0 ns | 59.85 % |                    65 B |
| Add      | 12000 | 109,444.5 ns | 33.51 % |                    65 B |
| Add      | 16000 | 144,011.4 ns | 24.00 % |                    65 B |
| Add      | 20000 | 188,569.9 ns | 23.63 % |                    65 B |
| Contains | 4000  |     554.5 ns |         |                       - |
| Contains | 8000  |     558.3 ns |  0.68 % |                       - |
| Contains | 12000 |   2,424.7 ns | 76.97 % |                       - |
| Contains | 16000 |   2,537.2 ns |  4.43 % |                       - |
| Contains | 20000 |   2,613.0 ns |  2.29 % |                       - |
| Remove   | 4000  |  26,838.4 ns |         |                       - |
| Remove   | 8000  |  71,494.9 ns | 62.46 % |                       - |
| Remove   | 12000 | 108,511.1 ns | 34.11 % |                       - |
| Remove   | 16000 | 148,685.2 ns | 27.02 % |                       - |
| Remove   | 20000 | 181,478.0 ns | 18.07 % |                       - |

**Mean:** The average time taken for a single execution of the method.

**Delta %** is the difference between the current and previous measurement (Mean) in percent. Used to compare the growth rate of the time taken for the operation with Logarithmic growth.

#### Logarithmic growth for the comparison

| N     | log(N) | Delta % |
|-------|-------:|--------:|
| 4000  |   3.60 |         |
| 8000  |   3.90 |  7.71 % |
| 12000 |   4.08 |  4.32 % |
| 16000 |   4.20 |  2.97 % |
| 20000 |   4.30 |  2.25 % |

### Based on the results, we can confirm that
- the time complexity of the **Add**, **Contains**, and **Remove** operations is **O(log n)**, as the time taken for the operation grows logarithmically with the number of elements.
- the space usage is **O(n)** for the **Add** operation as each operation allocates same amount of memory.
- the space usage for the **Contains** and **Remove** operations is constant, as it does not grow with the number of elements (except for the stack space used for the recursion).
