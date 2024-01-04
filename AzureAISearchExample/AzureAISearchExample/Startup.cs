using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AzureAISearchExample.RazorLib.Data;
using AzureAISearchExample.CoreLib;
using CoreLibrary;
using Microsoft.Extensions.Configuration;

namespace AzureAISearchExample;

public class Startup
{
    public static IServiceProvider? Services { get; private set; }

    public static void Init()
    {
        var host = Host.CreateDefaultBuilder()
                       .ConfigureServices(WireupServices)
                       .ConfigureAppConfiguration((hostingContext, config) =>
                       {
                           config.AddUserSecrets<Startup>();
                       })
                       .Build();
        Services = host.Services;
    }

    private static void WireupServices(IServiceCollection services)
    {
        services.AddWindowsFormsBlazorWebView();
        services.AddSingleton<WeatherForecastService>();
        services.AddScoped<MemoryService>();
        services.AddSingleton<FilePickerService>();
        services.AddScoped<AiSearchService>();
        services.AddSingleton<ChatService>();
#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif
    }
}
