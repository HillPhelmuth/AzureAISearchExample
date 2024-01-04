using Azure.Search.Documents.Models;

namespace AzureAISearchExample.CoreLib;

public record HybridResult(IEnumerable<SearchResult<IndexModel>> MemoryRecordMetadataResults, IEnumerable<QueryAnswerResult> SemanticResults);