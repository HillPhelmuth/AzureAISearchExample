using AzureAISearchExample.CoreLib;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Search.Documents.Models;
using Microsoft.SemanticKernel.Memory;

namespace AzureAISearchExample.RazorLib.Components
{
    public partial class HybridResultGrid : ComponentBase
    {
        [Parameter]
        public HybridResult? HybridResult { get; set; }
        [Parameter]
        public string ViewText { get; set; } = "";
        [Parameter]
        public EventCallback<string> ViewTextChanged { get; set; }
        private IEnumerable<SearchResult<IndexModel>> _searchResults = [];
        private IEnumerable<QueryAnswerResult> _semanticSearchResults = [];
        protected override Task OnParametersSetAsync()
        {
            if (HybridResult == null) return base.OnParametersSetAsync();
            _searchResults = HybridResult.MemoryRecordMetadataResults;
            _semanticSearchResults = HybridResult.SemanticResults;
            StateHasChanged();
            return base.OnParametersSetAsync();
        }
        private void HandleViewText(string item)
        {
            ViewText = item;
            ViewTextChanged.InvokeAsync(item);
            StateHasChanged();
        }
    }
}
