namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

using Microsoft.TeamFoundation.SourceControl.WebApi;
using Models;

internal interface IAzureDevOpsPullRequestService
{
    Task<IReadOnlyList<AzureDevOpsPullRequestContext>> GetPullRequestsForRepository(GitRepository repository);
}