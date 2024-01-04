using AzureAISearchExample.CoreLib;
using AzureAISearchExample.RazorLib.Data;
using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;

namespace AzureAISearchExample;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
     
        var blazor = new BlazorWebView()
        {
            Dock = DockStyle.Fill,
            HostPage = "wwwroot/index.html",
            Services = Startup.Services!,
            StartPath = "/"
        };
        
        blazor.RootComponents.Add<Main>("#app");
        Controls.Add(blazor);
    }
}
