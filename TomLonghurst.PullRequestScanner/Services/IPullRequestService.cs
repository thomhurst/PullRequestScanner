using TomLonghurst.PullRequestScanner.Models.Self;

namespace TomLonghurst.PullRequestScanner.Services;

internal interface IPullRequestService
{
    Task<IReadOnlyList<PullRequest>> GetPullRequests();
}