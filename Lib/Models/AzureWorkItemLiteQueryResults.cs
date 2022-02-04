using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Lib.Models;

public class AzureWorkItemLiteQueryResults
{
    [JsonProperty("workItems")]
    [JsonPropertyName("workItems")]
    public List<AzureWorkItem>? WorkItems { get; set; }
}