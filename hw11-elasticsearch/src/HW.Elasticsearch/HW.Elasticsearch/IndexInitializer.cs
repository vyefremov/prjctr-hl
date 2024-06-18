using Elastic.Clients.Elasticsearch;

namespace HW.Elasticsearch;

public class IndexInitializer(ElasticsearchClient elasticClient)
{
    public async Task InitializeAsync()
    {
        Console.WriteLine("InitializeAsync()");

        var words = await File.ReadAllLinesAsync($"{Directory.GetCurrentDirectory()}/Content/english.txt");

        Console.WriteLine($"Read {words.Length} words from file.");

        foreach (var batch in Batch(words, 1024))
        {
            await elasticClient.BulkAsync(
                d => d
                    .UpdateMany(
                        batch,
                        (b, l) => b
                            .Id(l.Id)
                            .Doc(l)
                            .DocAsUpsert()
                            .RetriesOnConflict(3))
                    .Refresh(Refresh.False));

            Console.WriteLine($"Inserted {batch.LastOrDefault()?.Id} words.");
        }
        
        Console.WriteLine("Finished inserting words.");
    }

    private static IEnumerable<List<WordDocument>> Batch(string[] source, int size)
    {
        // return batches
        var batch = new List<WordDocument>(size);
        
        for (int i = 0; i < source.Length; i++)
        {
            batch.Add(new WordDocument { Id = i, Value = source[i] });

            if (batch.Count != size) continue;
            
            var tmp = batch;

            batch = new List<WordDocument>(size);
                
            yield return tmp;
        }
        
        if (batch.Count > 0)
        {
            yield return batch;
        }
    }
}