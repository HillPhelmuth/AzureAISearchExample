using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Connectors.Memory.AzureAISearch;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Text;
using UglyToad.PdfPig;
using Encoding = Tiktoken.Encoding;

namespace CoreLibrary;

public class MemoryService
{
    private IMemoryStore _memoryStore;
    private ISemanticTextMemory _semanticTextMemory;
    private IConfiguration _config;
    private const string OutputPath = @"C:\Users\adamh\source\repos\AzureAISearchExample\AzureAISearchExample\AzureAISearchExample\OutputData";
    public event Action<string>? LogItem;
    private IServiceProvider? _serviceProvider;
    public List<string> GetChunkedFilePaths => Directory.GetFiles(OutputPath).ToList();
    public MemoryService(IConfiguration config)
    {
        _config = config;
        var services = new ServiceCollection();
        services.AddOpenAIChatCompletion("gpt-3.5-turbo-1106", _config["OpenAI:ApiKey"]!);
        services.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002", _config["OpenAI:ApiKey"]!);
        _serviceProvider = services.BuildServiceProvider();
        _memoryStore = new AzureAISearchMemoryStore(config["AzureAISearch:Endpoint"]!, config["AzureAISearch:QueryKey"]!);
        _semanticTextMemory = new MemoryBuilder()
            .WithMemoryStore(_memoryStore)
            .WithOpenAITextEmbeddingGeneration("text-embedding-ada-002", config["OpenAI:ApiKey"]!)
            .Build();
    }
    public async Task SaveToAzureAiSearch(List<MemoryItem> memoryItems, string indexName)
    {
        _memoryStore = new AzureAISearchMemoryStore(_config["AzureAISearch:Endpoint"]!, _config["AzureAISearch:ApiKey"]!);
        _semanticTextMemory = new MemoryBuilder()
            .WithMemoryStore(_memoryStore)
            .WithOpenAITextEmbeddingGeneration("text-embedding-ada-002", _config["OpenAI:ApiKey"]!)
            .Build();
        foreach (var item in memoryItems)
        {
            var text = item.Text.Replace(@"C:\Users\adamh\Downloads\azure-search.pdf", "");
            text = $"{item.Title}\n{text}";
            await _semanticTextMemory.SaveInformationAsync(indexName, text, item.Id, item.Title);
            LogItem?.Invoke($"Saved {item.Id} - {item.Title} to Azure AI Search");
        }
    }
    public async Task<IEnumerable<MemoryQueryResult>> SearchAzureAiVectorSearch(string query, int result = 5,
        double minSimilarity = 0.7d)
    {
        _memoryStore = new AzureAISearchMemoryStore(_config["AzureAISearch:Endpoint"]!, _config["AzureAISearch:ApiKey"]!);
        _semanticTextMemory = new MemoryBuilder()
            .WithMemoryStore(_memoryStore)
            .WithOpenAITextEmbeddingGeneration("text-embedding-ada-002", _config["OpenAI:ApiKey"]!)
            .Build();
        var results = await _semanticTextMemory.SearchAsync(AppConstants.IndexName, query, result, minSimilarity).ToListAsync();
        return results.OrderByDescending(x => x.Relevance);
    }
    public async Task ChunkAndSaveFile(byte[] file, string filename, FileType fileType)
    {
        var kernel = new Kernel(_serviceProvider);
        //var kernelBuilder = new KernelBuilder();
        //kernelBuilder.Services.AddLogging(builder => builder.AddConsole());        
        //Kernel kernel = kernelBuilder
        //    .AddOpenAIChatCompletion("gpt-3.5-turbo-1106", _config["OpenAI:ApiKey"]!)
        //    .Build();
        var titleGen = kernel.CreateFunctionFromPrompt("Generate a 5-10 word title for the following text:\n\n{{$text}} ", new OpenAIPromptExecutionSettings { Temperature = 0, TopP = 0.0, MaxTokens = 128 });
        var paragraphs = await ReadAndChunkFile(file, filename, fileType);
        var index = 1;
        foreach (var paragraph in paragraphs)
        {
            var funcResult = await titleGen.InvokeAsync(kernel, new KernelArguments { ["text"] = paragraph });
            var title = funcResult.ToString();
            var id = index.ToString();
            var memoryItem = new MemoryItem(id, title, paragraph);
            var memoryJson = JsonSerializer.Serialize(memoryItem, new JsonSerializerOptions { WriteIndented = true });
            var fileName = $"{RemoveNonAlphaNumericCharacters(paragraph)}.json";
            await File.WriteAllTextAsync(Path.Combine(OutputPath, fileName), memoryJson);
            LogItem?.Invoke($"Saved {id} - {title} to {fileName}");
            index++;
        }
    }
    public static string RemoveNonAlphaNumericCharacters(string input)
    {
        return Regex.Replace(input, "[^a-zA-Z0-9]", "");
    }
    private static async Task<List<string>> ReadAndChunkFile(byte[] file, string filename, FileType fileType)
    {
        var sb = new StringBuilder();
        switch (fileType)
        {
            case FileType.Pdf:
                {
                    using var document = PdfDocument.Open(file, new ParsingOptions { UseLenientParsing = true });
                    foreach (var page in document.GetPages())
                    {
                        var pageText = page.Text;
                        sb.Append(pageText);
                    }

                    break;
                }
            case FileType.Text:
                {
                    var stream = new StreamReader(new MemoryStream(file));
                    var text = await stream.ReadToEndAsync();
                    sb.Append(text);
                    break;
                }
        }

        var textString = sb.ToString();
        var lines = TextChunker.SplitPlainTextLines(textString, 128, StringHelpers.GetTokens);
        var paragraphs = TextChunker.SplitPlainTextParagraphs(lines, 512, 128, filename, StringHelpers.GetTokens);
        return paragraphs;
    }
}
public enum FileType
{
    Pdf,
    Text
}
public record MemoryItem(string Id, string Title, string Text);
public static class StringHelpers
{
    private static Encoding _encoding = Encoding.ForModel("gpt-4");
    public static int GetTokens(string text)
    {
        return _encoding.CountTokens(text);
    }
    public static string ExtractBase64FromDataUrl(this string dataUrl)
    {
        if (string.IsNullOrEmpty(dataUrl) || !dataUrl.StartsWith("data:"))
        {
            throw new ArgumentException("Invalid data URL.");
        }

        if (!dataUrl.Contains(";base64,"))
        {
            throw new ArgumentException("Not a base64 data URL.");
        }

        return dataUrl.Split(',').Last();
    }
}
internal class AppConstants
{
    public const string IndexName = "azureaisearchexample";
    public const string SemanticConfig = "semconfig";
}