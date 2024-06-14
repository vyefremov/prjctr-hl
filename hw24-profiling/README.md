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
For profiling purposes, I'd use **dotTrace** to measure execution time; and **dotMemory** to measure memory usage (based on dump).

Entry point is the [Program](./src/Profiling/Console/Program.cs) class.

### Result table

| Method   | N     | Mean us | Delta % | Allocated per operation |
|----------|-------|--------:|--------:|------------------------:|
| Add      | 4000  |   24.44 |         |                   776 B |
| Add      | 8000  |   70.65 | +65.41% |                   776 B |
| Add      | 12000 |  106.56 | +33.70% |                   776 B |
| Add      | 16000 |  144.16 | +26.08% |                   776 B |
| Add      | 20000 |  182.93 | +21.19% |                   776 B |
| Contains | 4000  |   11.59 |         |                         |
| Contains | 8000  |   12.98 | +10.71% |                         |
| Contains | 12000 |   13.84 |  +6.21% |                         |
| Contains | 16000 |   14.40 |  +3.89% |                         |
| Contains | 20000 |   14.75 |  +2.37% |                         |
| Remove   | 4000  |   32.08 |         |                         |
| Remove   | 8000  |   73.25 | +56.20% |                         |
| Remove   | 12000 |  109.12 | +32.87% |                         |
| Remove   | 16000 |  144.84 | +24.66% |                         |
| Remove   | 20000 |  182.03 | +20.43% |                         |

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
