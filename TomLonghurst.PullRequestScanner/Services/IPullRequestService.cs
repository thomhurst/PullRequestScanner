namespace TomLonghurst.PullRequestScanner.Services;

using TomLonghurst.PullRequestScanner.Models;

internal interface IPullRequestService
{
    Task<IReadOnlyList<PullRequest>> GetPullRequests();
}