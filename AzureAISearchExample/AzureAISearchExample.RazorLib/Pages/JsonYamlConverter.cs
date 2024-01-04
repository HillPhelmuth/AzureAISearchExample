using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AzureAISearchExample.RazorLib.Pages;

public static class JsonYamlConverter
{
    public static string ConvertJsonToYaml(string json, Dictionary<string, object> additionalProperties = null)
    {
        var jsonObject = JObject.Parse(json);

        if (additionalProperties != null)
        {
            foreach (var property in additionalProperties)
            {
                jsonObject.Add(new JProperty(property.Key, property.Value));
            }
        }

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        var yamlObject = deserializer.Deserialize<object>(jsonObject.ToString());

        var serializer = new Serializer();
        using var writer = new StringWriter();
        serializer.Serialize(writer, yamlObject);
        var yaml = writer.ToString();
        return yaml;
    }
}