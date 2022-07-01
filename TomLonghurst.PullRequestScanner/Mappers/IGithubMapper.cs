using TomLonghurst.PullRequestScanner.Models.Github;
using TomLonghurst.PullRequestScanner.Models.Self;

namespace TomLonghurst.PullRequestScanner.Mappers;

internal interface IGithubMapper
{
    PullRequest ToPullRequestModel(GithubPullRequest githubPullRequest);
}