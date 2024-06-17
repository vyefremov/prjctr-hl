namespace App;

public class CountingSort
{
    public static int[] Sort(int[] input)
    {
        var n = input.Length;
        var max = input.Max();
        var output = new int[n];
        var count = new int[max + 1];

        for (var i = 0; i < input.Length; i++)
        {
            count[input[i]]++;
        }

        for (var i = 1; i < count.Length; i++)
        {
            // Cumulative sum or prefix sum to make sorting stable
            count[i] = count[i - 1] + count[i];
        }
        
        for (var i = n - 1; i >= 0; i--)
        {
            // Make the sorting stable
            output[count[input[i]] - 1] = input[i];
            count[input[i]]--;
        }

        return output;
    }
}

public class CountingSortCheck
{
    public static void Run()
    {
        Console.WriteLine("Counting Sort");
        
        int[] input = [4, 2, 2, 8, 3, 3, 1];

        Console.WriteLine($"Input: [{string.Join(',', input)}]");

        var output = CountingSort.Sort(input);

        Console.WriteLine($"Output: [{string.Join(',', output)}]");
    }
}
