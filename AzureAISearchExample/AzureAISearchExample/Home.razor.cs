using System.Text.Json;
using AzureAISearchExample.CoreLib;
using AzureAISearchExample.CoreLib.Helpers;
using CoreLibrary;
using Microsoft.AspNetCore.Components;
using Microsoft.SemanticKernel.Memory;
using Radzen;
using MemoryItem = CoreLibrary.MemoryItem;

namespace AzureAISearchExample
{
    public partial class Home
    {
        private const string IndexName = "azureaisearchexample2";

        [Inject]
        private MemoryService MemoryService { get; set; } = default!;
        [Inject]
        private AiSearchService AiSearchService { get; set; } = default!;
        [Inject]
        private FilePickerService FilePickerService { get; set; } = default!;
        private bool _isBusy;
        private class FileUploadData
        {
            public string? FileName { get; init; }
            public long? FileSize { get; set; }
            public byte[]? File { get; init; }
            public const int MaxFileSize = int.MaxValue;

        }
        private class SearchForm
        {
            public string? SearchText { get; set; }
            public int MaxResults { get; set; } = 5;
            public double MinThreshold { get; set; } = 0.7d;
        }
        private SearchForm _searchForm = new();
        private List<MemoryQueryResult> _searchResults = new();
        private HybridResult? _hybridResult;
        private async void Search(SearchForm searchForm)
        {
            _searchResults = (await MemoryService.SearchAzureAiVectorSearch(searchForm.SearchText!, searchForm.MaxResults, searchForm.MinThreshold)).ToList();
            _hybridResult = await AiSearchService.SearchAzureAiHybridSearch(searchForm.SearchText!, searchForm.MaxResults);
            StateHasChanged();
        }
        private async void HybridSearch(SearchForm searchForm)
        {
            _searchResults = (await MemoryService.SearchAzureAiVectorSearch(searchForm.SearchText!, searchForm.MaxResults, searchForm.MinThreshold)).ToList();
            _hybridResult = await AiSearchService.SearchAzureAiHybridSearch(searchForm.SearchText!, searchForm.MaxResults);
            StateHasChanged();
        }
        private class FileUploadForm
        {
            public List<FileUploadData> Files { get; set; } = new();
        }
        private string _viewText = "";
        private List<MemoryItem> _memoryItems = [];
        private FileUploadForm _fileUploadForm = new();
        private List<string> _logList = [];
        //private FileUploadData _fileUploadData = new();
        protected override Task OnInitializedAsync()
        {
            MemoryService.LogItem += HandleLog;
            AiSearchService.LogItem += HandleLog;
            var files = MemoryService.GetChunkedFilePaths;
            _memoryItems = files.Select(x => JsonSerializer.Deserialize<MemoryItem>(File.ReadAllText(x))!).ToList();
            return base.OnInitializedAsync();
        }
        private void HandleViewText(string item)
        {
            _viewText = item;
            StateHasChanged();
        }
        private async Task SaveAll()
        {
            _isBusy = true;
            StateHasChanged();
            await Task.Delay(1);
            await MemoryService.SaveToAzureAiSearch(_memoryItems, IndexName);
            _isBusy = false;
            StateHasChanged();
        }
        private void HandleFileSelection()
        {
            var filePickerResult = FilePickerService.ShowOpenFileDialog();
            if (filePickerResult.Count == 0) return;
            foreach (var (fileName, fileBytes) in filePickerResult)
            {
                var fileUploadData = new FileUploadData
                {
                    FileName = fileName,
                    FileSize = fileBytes.Length,
                    File = fileBytes
                    
                };
                _fileUploadForm.Files.Add(fileUploadData);
            }
            StateHasChanged();
        }
        private async Task ProcessFiles()
        {
            _isBusy = true;
            StateHasChanged();
            await Task.Delay(1);
            foreach (var fileUploadData in _fileUploadForm.Files)
            {
                var file = fileUploadData.File;
                await MemoryService.ChunkAndSaveFile(file!, fileUploadData.FileName!, FileType.Pdf);
            }
            //await MemoryService.ChunkAndSaveFile(file, fileUploadData.FileName!, FileType.Pdf);
            
            _isBusy = false;
            StateHasChanged();
        }
        private void HandleError(UploadErrorEventArgs args)
        {
            var error = args.Message;
            _logList.Add($"FILE ERROR: {error}");
        }
        private async void HandleLog(string logItem)
        {
            _logList.Add(logItem);
            await InvokeAsync(StateHasChanged);
        }
    }
}
