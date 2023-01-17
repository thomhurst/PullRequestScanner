using TomLonghurst.PullRequestScanner.GitHub.Models;

namespace TomLonghurst.PullRequestScanner.GitHub.Services;

internal interface IGithubPullRequestService
{
    Task<IEnumerable<GithubPullRequest>> GetPullRequests(GithubRepository repository);
}