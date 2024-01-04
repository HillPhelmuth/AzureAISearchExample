namespace AzureAISearchExample.CoreLib;

public record MemoryItem(string Id, string Title, string Text)
{
    public bool IsSelected { get; set; }
}