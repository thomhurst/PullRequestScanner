using TomLonghurst.PullRequestScanner.Models;

namespace TomLonghurst.PullRequestScanner.Services;

internal interface IPullRequestService
{
    Task<IReadOnlyList<PullRequest>> GetPullRequests();
}