// See https://aka.ms/new-console-template for more information

using Elastic.Clients.Elasticsearch;
using HW.Elasticsearch;

var client = ElasticsearchClientBuilder.CreateClient();

var indexBuilder = new IndexManager(client);
var indexInitializer = new IndexInitializer(client);

while (true)
{
    Console.WriteLine();
    Console.WriteLine("Type 'exit' to quit, 'clear' to clear the console, 'index' to rebuild index, any other command to search.");
    Console.Write("Command: ");
    
    var command = Console.ReadLine();
    
    if (command == "exit")
    {
        break;
    }

    if (command == "clear")
    {
        Console.Clear();
        continue;
    }
    
    if (command == "index")
    {
        await indexBuilder.DeleteIndexAsync();
        await indexBuilder.CreateIndexAsync();
        // await indexInitializer.InitializeAsync();
        continue;
    }

    if (command is not { Length: > 0 })
    {
        continue;
    }

    var searchLine = command.Trim();

    SearchResponse<WordDocument> response;

    if (searchLine.Length > 7)
    {
        var minimumShouldMatch = (int) Math.Round((searchLine.Length - IndexManager.AllowedTypos - 1) * 100.0 / searchLine.Length);

        Console.WriteLine("Minimum should match: " + minimumShouldMatch);

        response = await client
            .SearchAsync<WordDocument>(s => s
                .Index(IndexManager.IndexName)
                .Query(q => q
                    .Bool(b => b
                        .Should(
                            sh => sh
                                .Match(m => m
                                    .Field(f => f.Value.Suffix(IndexManager.NgramField))
                                    .Query(searchLine)
                                    .Analyzer(IndexManager.NgramAnalyzer)
                                    .MinimumShouldMatch($"{minimumShouldMatch}%")))
                        .Must(m => m
                            .Script(scr => scr
                                .Script(new Script(
                                    new InlineScript(
                                        $"doc['Value.keyword'].value.length() >= {searchLine.Length}"))))))));
    }
    else
    {
        response = await client
            .SearchAsync<WordDocument>(s => s
                .Index(IndexManager.IndexName)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.Value.Suffix(IndexManager.AutocompleteField))
                        .Query(searchLine)
                        .Fuzziness(new Fuzziness("AUTO")))));
    }

    Console.WriteLine("Found words: ");
        
    foreach (var word in response.Documents)
    {
        Console.WriteLine(word.Value);
    }
}
