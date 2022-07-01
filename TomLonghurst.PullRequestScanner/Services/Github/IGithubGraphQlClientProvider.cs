using Octokit.GraphQL;

namespace TomLonghurst.PullRequestScanner.Services.Github;

internal interface IGithubGraphQlClientProvider
{
    Connection GithubGraphQlClient { get; }
}