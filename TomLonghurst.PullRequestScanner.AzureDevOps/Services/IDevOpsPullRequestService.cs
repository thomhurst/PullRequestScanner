using Microsoft.TeamFoundation.SourceControl.WebApi;
using TomLonghurst.PullRequestScanner.AzureDevOps.Models;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

internal interface IAzureDevOpsPullRequestService
{
    Task<IReadOnlyList<AzureDevOpsPullRequestContext>> GetPullRequestsForRepository(GitRepository repository);
}