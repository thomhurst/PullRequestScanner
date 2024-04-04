namespace TomLonghurst.PullRequestScanner.Services;

using Models;

internal interface IPullRequestService
{
    Task<IReadOnlyList<PullRequest>> GetPullRequests();
}