using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch.IndexManagement;

namespace HW.Elasticsearch;

public class IndexManager(ElasticsearchClient elasticClient)
{
    public const string IndexName = "word";

    public const int AllowedTypos = 3;
    
    private const int NgramMin = 1;
    private const int NgramMax = 3;
    
    public const string NgramField = "ngram";
    private const string NgramTokenizer = "ngram_tokenizer";
    public const string NgramAnalyzer = "ngram_analyzer";
    
    public const string EdgeNgramField = "edgengram";
    private const string EdgeNgramTokenizer = "edge_ngram_tokenizer";
    private const string EdgeNgramAnalyzer = "edge_ngram_analyzer";
    private const string EdgeNgramFilter = "edge_ngram_filter";
    
    public const string AutocompleteField = "autocomplete";
    private const string AutocompleteAnalyzer = "autocomplete_analyzer";
    
    private const string StandardTokenizer = "standard";
    private const string LowerCaseFilter = "lowercase";
    private const string SearchAnalyzer = "search_analyzer";
    
    public async Task CreateIndexAsync(string indexName = IndexName)
    {
        Console.WriteLine($"CreateIndexAsync({indexName})");
        
        var exists = await elasticClient.Indices.ExistsAsync(indexName);
        
        if (exists.Exists)
        {
            Console.WriteLine($"Index {indexName} already exists.");
            
            return;
        }

        var response = await CreateV1(indexName);

        if (!response.IsValidResponse || !response.IsSuccess())
        {
            throw new Exception($"Failed to create index {indexName}. {response.DebugInformation}.");
        }
    }

    private Task<CreateIndexResponse> CreateV1(string indexName)
    {
        return elasticClient.Indices
            .CreateAsync(
                indexName,
                request => request
                    .Settings(s => s
                        .MaxNgramDiff(NgramMax - NgramMin)
                        .RefreshInterval(TimeSpan.FromSeconds(10))
                        .Analysis(analysis => analysis
                            .Tokenizers(t => t.NGram(NgramTokenizer,
                                    c => c
                                        .MinGram(NgramMin)
                                        .MaxGram(NgramMax)
                                        .TokenChars([TokenChar.Letter, TokenChar.Digit]))
                                // .EdgeNGram(EdgeNgramTokenizer,
                                //     c => c
                                //         .MinGram(1)
                                //         .MaxGram(20)
                                //         .TokenChars([TokenChar.Letter, TokenChar.Digit]))
                            )
                            .TokenFilters(tf => tf.EdgeNGram(EdgeNgramFilter, c => c
                                .MinGram(1)
                                .MaxGram(20))
                            )
                            .Analyzers(a => a
                                .Custom(AutocompleteAnalyzer, aa => aa
                                    .Tokenizer(StandardTokenizer)
                                    .Filter([LowerCaseFilter, EdgeNgramFilter]))
                                .Custom(NgramAnalyzer, aa => aa
                                    .Tokenizer(NgramTokenizer)
                                    .Filter([LowerCaseFilter]))
                                // .Custom(EdgeNgramAnalyzer, aa => aa
                                //     .Tokenizer(EdgeNgramTokenizer)
                                //     .Filter([LowerCaseFilter]))
                                .Custom(SearchAnalyzer, aa => aa
                                    .Tokenizer(StandardTokenizer)
                                    .Filter([LowerCaseFilter]))))
                    )
                    .Mappings(m => m.Properties<WordDocument>(props => props
                        .Text(x => x.Value, tpd => tpd
                            .Fields(fpd => fpd
                                .Keyword("keyword")
                                .Text(AutocompleteField, tf => tf
                                    .Analyzer(AutocompleteAnalyzer)
                                    .SearchAnalyzer(SearchAnalyzer))
                                .Text(NgramField, tf => tf
                                    .Analyzer(NgramAnalyzer)
                                    .SearchAnalyzer(SearchAnalyzer)))))
                    )
            );
    }

    public async Task DeleteIndexAsync(string indexName = IndexName)
    {
        Console.WriteLine($"DeleteIndexAsync({indexName})");
        
        var exists = await elasticClient.Indices.ExistsAsync(indexName);
        
        if (!exists.Exists)
        {
            Console.WriteLine($"Index {indexName} not exists.");
            
            return;
        }
        
        var response = await elasticClient.Indices.DeleteAsync(indexName);
        
        if (!response.IsValidResponse || !response.IsSuccess())
        {
            throw new Exception($"Failed to delete index {indexName}. {response.DebugInformation}.");
        }
    }
}
