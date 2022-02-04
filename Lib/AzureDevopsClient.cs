using System.Net.Http.Headers;
using System.Text;
using Lib.Configuration;
using Lib.Models;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Lib;

public class AzureDevopsClient : IAzureDevopsClient
{
    private readonly HttpClient _httpClient;
    private readonly WorkItemTrackingHttpClient _sdkClient;
    private readonly IOptions<Settings> _options;

    public AzureDevopsClient(HttpClient httpClient, WorkItemTrackingHttpClient sdkClient, IOptions<Settings> options)
    {
        _options = options;
        _sdkClient = sdkClient;
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Accept.Add(GetAcceptHeader());
        _httpClient.DefaultRequestHeaders.Authorization = GetAuthorizationHeader();
    }

    public Task<ICollection<AzureWorkItem>> GetAzureWorkItemsAsync(AzureDevopsChoice choice, CancellationToken cancellationToken = default)
    {
        return choice switch
        {
            AzureDevopsChoice.Sdk => GetBySdkAsync(cancellationToken),
            AzureDevopsChoice.Rest => GetByRestAsync(cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, $"Unexpected choice {choice}")
        };
    }

    private async Task<ICollection<AzureWorkItem>> GetBySdkAsync(CancellationToken cancellationToken = default)
    {
        var query = new Wiql { Query = _options.Value.WorkItemQuery };
        var liteResults = await _sdkClient.QueryByWiqlAsync(query, cancellationToken: cancellationToken);
        var ids = liteResults.WorkItems.Select(x => x.Id).ToList();
        if (!ids.Any())
        {
            return new List<AzureWorkItem>();
        }

        var fullResults = await _sdkClient.GetWorkItemsAsync(ids, cancellationToken: cancellationToken);
        var workItems = fullResults.Select(x => new AzureWorkItem
        {
            Id = x.Id!.Value,
            Url = x.Url,
            Details = new AzureWorkItemDetails
            {
                Title = x.Fields["System.Title"].ToString(),
                State = x.Fields["System.State"].ToString(),
                Type = x.Fields["System.WorkItemType"].ToString()
            }
        }).ToList();
        return workItems;
    }

    public async Task<ICollection<AzureWorkItem>> GetByRestAsync(CancellationToken cancellationToken = default)
    {
        var query = _options.Value.WorkItemQuery;
        var content = new StringContent("{ \"query\": \"" + query + "\" }", Encoding.UTF8, "application/json");
        var liteRequestUrl = _options.Value.WorkItemsLiteQueryUrl;
        using var liteResponse = await _httpClient.PostAsync(liteRequestUrl, content, cancellationToken);
        liteResponse.EnsureSuccessStatusCode();
        var liteResults = await liteResponse.Content.ReadAsAsync<AzureWorkItemLiteQueryResults>(cancellationToken);
        var ids = liteResults.WorkItems?.Select(x => x.Id).ToList() ?? new List<int>();
        if (!ids.Any())
        {
            return new List<AzureWorkItem>();
        }

        var fullRequestUrl = _options.Value.WorkItemsFullQueryUrl(ids);
        using var fullResponse = await _httpClient.GetAsync(fullRequestUrl, cancellationToken);
        fullResponse.EnsureSuccessStatusCode();
        var fullResults = await fullResponse.Content.ReadAsAsync<AzureWorkItemFullQueryResults>(cancellationToken);
        return fullResults.WorkItems ?? new List<AzureWorkItem>();
    }

    private static MediaTypeWithQualityHeaderValue GetAcceptHeader() => new MediaTypeWithQualityHeaderValue("application/json");

    private AuthenticationHeaderValue GetAuthorizationHeader()
    {
        var pat = _options.Value.PersonalAccessToken;
        var bytes = Encoding.ASCII.GetBytes($":{pat}");
        var base64 = Convert.ToBase64String(bytes);
        return new AuthenticationHeaderValue("Basic", base64);
    }
}