using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace HW.Elasticsearch;

public static class ElasticsearchClientBuilder
{
    public static ElasticsearchClient CreateClient(
        string url = "http://localhost:9200",
        string apiKey = "")
    {
        var settings = new ElasticsearchClientSettings(new Uri(url))
            .DefaultFieldNameInferrer(p => p)
            .Authentication(new ApiKey(apiKey))
            .ThrowExceptions()
            .DisableDirectStreaming()
            .OnRequestCompleted(r =>
            {
                Console.WriteLine(r.DebugInformation);
            })
            .DefaultMappingFor<WordDocument>(d => d.IndexName(IndexManager.IndexName));

        return new ElasticsearchClient(settings);
    }
}