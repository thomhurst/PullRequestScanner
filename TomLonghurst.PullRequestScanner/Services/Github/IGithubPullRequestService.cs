using TomLonghurst.PullRequestScanner.Models.Github;

namespace TomLonghurst.PullRequestScanner.Services.Github;

internal interface IGithubPullRequestService
{
    Task<IEnumerable<GithubPullRequest>> GetPullRequests(GithubRepository repository);
}