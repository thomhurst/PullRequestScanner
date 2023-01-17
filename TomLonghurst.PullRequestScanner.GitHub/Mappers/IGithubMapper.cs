using TomLonghurst.PullRequestScanner.GitHub.Models;
using TomLonghurst.PullRequestScanner.Models;

namespace TomLonghurst.PullRequestScanner.GitHub.Mappers;

internal interface IGithubMapper
{
    PullRequest ToPullRequestModel(GithubPullRequest githubPullRequest);
}