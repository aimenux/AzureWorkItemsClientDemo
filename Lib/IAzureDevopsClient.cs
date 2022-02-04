using Lib.Models;

namespace Lib;

public interface IAzureDevopsClient
{
    Task<ICollection<AzureWorkItem>> GetAzureWorkItemsAsync(AzureDevopsChoice choice, CancellationToken cancellationToken = default);
}