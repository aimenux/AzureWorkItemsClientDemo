namespace Lib.Configuration;

public class Settings
{
    public string ApiVersion { get; set; } = "api-version=6.0";

    public string? OrganizationName { get; set; }

    public string? PersonalAccessToken { get; set; }

    public string AzureDevopsUrl => @"https://dev.azure.com";

    public string WorkItemQuery { get; set; } = "Select [System.Id], [System.Title], [System.State] From WorkItems Where [State] <> 'Closed' order by [System.CreatedDate] ASC";

    public string WorkItemsLiteQueryUrl => $"https://dev.azure.com/{OrganizationName}/_apis/wit/wiql?{ApiVersion}";

    public string WorkItemsFullQueryUrl(List<int> workItemIds) => $"https://dev.azure.com/{OrganizationName}/_apis/wit/workitems?ids={string.Join(",", workItemIds)}&{ApiVersion}";
}