using Tiktoken;


namespace AzureAISearchExample.CoreLib.Helpers;

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
public class AppConstants
{
    public const string IndexName = "azureaisearchexample";
    public const string SemanticConfig = "semconfig";
}