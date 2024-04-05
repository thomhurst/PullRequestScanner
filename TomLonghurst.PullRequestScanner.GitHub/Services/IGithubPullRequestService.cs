namespace TomLonghurst.PullRequestScanner.GitHub.Services;

using Models;

internal interface IGithubPullRequestService
{
    Task<IEnumerable<GithubPullRequest>> GetPullRequests(GithubRepository repository);
}