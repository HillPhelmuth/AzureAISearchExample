using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AzureAISearchExample.CoreLib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.PromptTemplate.Handlebars;

namespace CoreLibrary;

public class ChatService
{
    public const string ChatPromptPath = @"C:\Users\adamh\source\repos\AzureAISearchExample\CoreLibrary\PluginFunctions\GroundedChat.prompt.yaml";
    public ChatHistory ChatHistory { get; set; } = new();
    private IConfiguration _config;
    private IServiceProvider? _serviceProvider;
    private readonly MemoryService _memoryService;
    private Kernel _kernel;
    private KernelFunction _chatFuction;
    private readonly AiSearchService _aiSearchService;
    public ChatService(IConfiguration config, MemoryService memoryService, AiSearchService aiSearchService)
    {
        _config = config;
        _memoryService = memoryService;
        _aiSearchService = aiSearchService;
        var services = new ServiceCollection();
        services.AddOpenAIChatCompletion("gpt-4-1106-preview", _config["OpenAI:ApiKey"]!);
        _serviceProvider = services.BuildServiceProvider();
        _kernel = new Kernel(_serviceProvider);
       _chatFuction =  _kernel.CreateFunctionFromPromptYaml(EmbeddedResource.Read("GroundedChat.prompt.yaml"), new HandlebarsPromptTemplateFactory());
    }
    public async IAsyncEnumerable<ChatMessageContent> Chat(string message, bool useHybrid = false)
    {
        ChatHistory.AddUserMessage(message);
        string top10String = "";
        if (useHybrid)
        {
            var search = await _aiSearchService.SearchAzureAiHybridSearch(message, 10);
            var answers = search.SemanticResults.Select(x => x.Text);
            var answerString = $"Potential Answers:{string.Join("\n\n", answers)}";
            var index = 1;
            top10String = $"{answerString}\n\n{index++}{string.Join($"\n\n{index++}", search.MemoryRecordMetadataResults.Select(x => x.Document.Text))}";
        }
        else
        {
            var top10 = await _memoryService.SearchAzureAiVectorSearch(message, 10, 0.75d);
            top10String = string.Join("\n\n", top10.Select(x => x.Metadata.Text));
        }
        
        var kernelVariable = new KernelArguments
        {
            ["messages"] = ChatHistory,
            ["persona"] = "You are an expert AI assistant",
            ["grounding"] = top10String           

        };
        IKernelPlugin? plugin = new KernelPlugin("ChatPlugin", new List<KernelFunction> { _chatFuction});
        _kernel.Plugins.Add(plugin);

        var assMessge = "";
        ChatHistory.AddAssistantMessage(assMessge);
        await foreach (var chat in _chatFuction.InvokeStreamingAsync<string>(_kernel, kernelVariable))
        {
            //assMessge += chat;
            ChatHistory.LastOrDefault().Content += chat;
            yield return ChatHistory.LastOrDefault();
        }
        
    }
    
}
internal static class EmbeddedResource
{
    private static readonly string? s_namespace = typeof(EmbeddedResource).Namespace;

    internal static string Read(string name)
    {
        //var assembly = typeof(EmbeddedResource).GetTypeInfo().Assembly;
        //if (assembly == null) { throw new FileNotFoundException($"[{s_namespace}] {name} assembly not found"); }

        //using Stream? resource = assembly.GetManifestResourceStream($"{s_namespace}." + name);
        //if (resource == null) { throw new FileNotFoundException($"[{s_namespace}] {name} resource not found"); }

        //using var reader = new StreamReader(resource);
        //return reader.ReadToEnd();
        return ExtractFromAssembly<string>(name);
    }
    public static T ExtractFromAssembly<T>(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var jsonName = assembly.GetManifestResourceNames()
            .SingleOrDefault(s => s.EndsWith(fileName, StringComparison.OrdinalIgnoreCase)) ?? "";
        using var stream = assembly.GetManifestResourceStream(jsonName);
        using var reader = new StreamReader(stream);
        object result = reader.ReadToEnd();
        if (typeof(T) == typeof(string))
            return (T)result;
        return JsonSerializer.Deserialize<T>(result.ToString());
    }
}