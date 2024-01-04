namespace AzureAISearchExample.CoreLib
{
    public class IndexModel
    {
        
        public bool IsReference { get; set; }

        /// <summary>
        /// A value used to understand which external service owns the data, to avoid storing the information
        /// inside the URI. E.g. this could be "MSTeams", "WebSite", "GitHub", etc.
        /// </summary>
        
        public string ExternalSourceName { get; set; }

        /// <summary>
        /// Unique identifier. The format of the value is domain specific, so it can be a URL, a GUID, etc.
        /// </summary>
      
        public string Id { get; set; }

        /// <summary>
        /// Optional title describing the content. Note: the title is not indexed.
        /// </summary>

        public string Description { get; set; }

        /// <summary>
        /// Source text, available only when the memory is not an external source.
        /// </summary>

        public string Text { get; set; }

        /// <summary>Field for saving custom metadata with a memory.</summary>

        public string AdditionalMetadata { get; set; }
        public List<float> Embeddings { get; set; }
    }
}
