namespace TomLonghurst.PullRequestScanner.GitHub.Services;

using TomLonghurst.PullRequestScanner.GitHub.Models;

internal interface IGithubPullRequestService
{
    Task<IEnumerable<GithubPullRequest>> GetPullRequests(GithubRepository repository);
}