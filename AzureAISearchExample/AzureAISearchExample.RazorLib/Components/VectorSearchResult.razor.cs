using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SemanticKernel.Memory;

namespace AzureAISearchExample.RazorLib.Components
{
    public partial class VectorSearchResult : ComponentBase
    {
        [Parameter]
        public List<MemoryQueryResult> SearchResults { get; set; } = [];
        [Parameter]
        public string ViewText { get; set; } = "";
        [Parameter]
        public EventCallback<string> ViewTextChanged { get; set; }
        private void HandleViewText(string item)
        {
            ViewText = item;
            ViewTextChanged.InvokeAsync(item);
            StateHasChanged();
        }
    }
}
