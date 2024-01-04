using Azure.Search.Documents.Models;
using Azure.Search.Documents;
using AzureAISearchExample.CoreLib.Helpers;
using Microsoft.Extensions.Configuration;
using Azure;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;

namespace AzureAISearchExample.CoreLib;

public class AiSearchService
{
    private readonly IConfiguration _config;
    private readonly SearchClient _searchClient;
    public event Action<string>? LogItem;
    public AiSearchService(IConfiguration config)
    {
        _config = config;
        _searchClient = new SearchClient(new Uri(config["AzureAISearch:Endpoint"]!), AppConstants.IndexName, new AzureKeyCredential(config["AzureAISearch:QueryKey"]!));
    }

    public async Task<HybridResult> SearchAzureAiHybridSearch(string query, int result = 5)
    {
        var embedServices = new OpenAITextEmbeddingGenerationService("text-embedding-ada-002", _config["OpenAI:ApiKey"]!);
        var embeddings = await embedServices.GenerateEmbeddingAsync(query);
        var searchOptions = new SearchOptions
        {
            Size = result,
            VectorSearch = new VectorSearchOptions
            {
                Queries = { new VectorizedQuery(embeddings) { KNearestNeighborsCount = result, Fields = { "Embedding" } } }
            },
            SemanticSearch = new SemanticSearchOptions
            {
                SemanticConfigurationName = AppConstants.SemanticConfig,
                QueryCaption = new QueryCaption(QueryCaptionType.Extractive),
                QueryAnswer = new QueryAnswer(QueryAnswerType.Extractive)
            },
            QueryType = SearchQueryType.Semantic,
        };
        SearchResults<IndexModel> response = await _searchClient.SearchAsync<IndexModel>(query, searchOptions);

        var semanticSearchAnswers = response.SemanticSearch.Answers;

        var metaDataitems = await response.GetResultsAsync().ToListAsync();
        return new HybridResult(metaDataitems, semanticSearchAnswers);
    }
}