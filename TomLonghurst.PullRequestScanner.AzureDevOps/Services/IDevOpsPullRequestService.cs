using TomLonghurst.PullRequestScanner.AzureDevOps.Models;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

internal interface IDevOpsPullRequestService
{
    Task<IReadOnlyList<DevOpsPullRequestContext>> GetPullRequestsForRepository(DevOpsGitRepository githubGitRepository);
}