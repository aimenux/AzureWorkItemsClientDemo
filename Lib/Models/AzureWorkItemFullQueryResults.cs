using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Lib.Models;

public class AzureWorkItemFullQueryResults
{
    [JsonProperty("count")]
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonProperty("value")]
    [JsonPropertyName("value")]
    public List<AzureWorkItem>? WorkItems { get; set; }
}