using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using CoreLibrary;
using Markdig;

namespace AzureAISearchExample.RazorLib.Pages;

public partial class Examples : ComponentBase
{
    private const string PluginPath = @"C:\Users\adamh\source\repos\AdventuresInSemanticKernel\SkPluginLibrary\SemanticPlugins";
    private const string OutputPath = @"C:\Users\adamh\source\repos\AdventuresInSemanticKernel\SkPluginLibrary\YamlPlugins";

    private IEnumerable<string> PluginDirectories => Directory.GetDirectories(PluginPath);
    private List<string> _log = [];
    private string _response = "";
    private string? _input;
    private class ChatInputForm
    {
        public string? Input { get; set; }
        public bool UseHybrid { get; set; }
    }
    private ChatInputForm _chatInput = new();
    [Inject]
    private ChatService ChatService { get; set; } = default!;
    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
         
        }
        return base.OnAfterRenderAsync(firstRender);
    }
    private async void SubmitChat(ChatInputForm chatInput)
    {
        _response = "";
        var response = ChatService.Chat(chatInput.Input, chatInput.UseHybrid);
        await foreach (var token in response)
        {
            //_response += token;
            await InvokeAsync(StateHasChanged);
        }
    }
    private static string MarkdownToHtml(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var result = Markdown.ToHtml(text, pipeline);
        return result;
    }
    private async Task Convert()
    {

        foreach (var pluginPath in PluginDirectories)
        {

            var pluginName = Path.GetFileName(pluginPath);
            _log.Add($"Start on plugin {pluginName}");
            foreach (var functionPath in Directory.GetDirectories(pluginPath))
            {
                var name = Path.GetFileName(functionPath);
                var config = Directory.GetFiles(functionPath, "config.json").FirstOrDefault();
                var configJson = await File.ReadAllTextAsync(config);
                var template = Directory.GetFiles(functionPath, "skprompt.txt").FirstOrDefault();
                var templateText = await File.ReadAllTextAsync(template);
                var additionalProps = new Dictionary<string, object> { { "name", name }, { "template", templateText } };
                var yaml = JsonYamlConverter.ConvertJsonToYaml(configJson, additionalProps);
                var yamlPath = Path.Combine(OutputPath, pluginName, $"{name}.yaml");
                if (!Directory.Exists(Path.Combine(OutputPath, pluginName)))
                {
                    Directory.CreateDirectory(Path.Combine(OutputPath, pluginName));
                }
                await File.WriteAllTextAsync(yamlPath, yaml);
                _log.Add($"converted {name}");
                StateHasChanged();
            }
            //var config = Directory.GetFiles(functionPath, "config.json").FirstOrDefault();
            //var configJson = await File.ReadAllTextAsync(config);
            //var template = Directory.GetFiles(functionPath, "skprompt.txt").FirstOrDefault();
            //var templateText = await File.ReadAllTextAsync(template);
            //var additionalProps = new Dictionary<string, object> { { "name", name }, { "template", templateText } };
            //var yaml = JsonYamlConverter.ConvertJsonToYaml(configJson, additionalProps);
            //var yamlPath = Path.Combine(OutputPath, $"{name}.yaml");
            //await File.WriteAllTextAsync(yamlPath, yaml);
            //_log.Add($"converted {name}");
        }
    }
    
}