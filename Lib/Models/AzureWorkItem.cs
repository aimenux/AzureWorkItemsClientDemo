using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Lib.Models;

public class AzureWorkItem
{
    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonProperty("url")]
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonProperty("fields")]
    [JsonPropertyName("fields")]
    public AzureWorkItemDetails? Details { get; set; }
}