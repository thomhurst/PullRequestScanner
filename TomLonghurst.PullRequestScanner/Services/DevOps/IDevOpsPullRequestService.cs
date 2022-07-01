using TomLonghurst.PullRequestScanner.Models.DevOps;

namespace TomLonghurst.PullRequestScanner.Services.DevOps;

internal interface IDevOpsPullRequestService
{
    Task<IReadOnlyList<DevOpsPullRequestContext>> GetPullRequestsForRepository(DevOpsGitRepository githubGitRepository);
}