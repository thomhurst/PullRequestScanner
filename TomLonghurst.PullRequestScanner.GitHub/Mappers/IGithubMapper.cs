namespace TomLonghurst.PullRequestScanner.GitHub.Mappers;

using Models;
using TomLonghurst.PullRequestScanner.Models;

internal interface IGithubMapper
{
    PullRequest ToPullRequestModel(GithubPullRequest githubPullRequest);
}