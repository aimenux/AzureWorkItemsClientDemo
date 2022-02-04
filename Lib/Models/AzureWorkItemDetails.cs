using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Lib.Models;

public class AzureWorkItemDetails
{
    [JsonProperty("System.State")]
    [JsonPropertyName("System.State")]
    public string? State { get; set; }

    [JsonProperty("System.WorkItemType")]
    [JsonPropertyName("System.WorkItemType")]
    public string? Type { get; set; }

    [JsonProperty("System.Title")]
    [JsonPropertyName("System.Title")]
    public string? Title { get; set; }
}