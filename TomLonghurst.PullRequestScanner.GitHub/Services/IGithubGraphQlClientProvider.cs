using Octokit.GraphQL;

namespace TomLonghurst.PullRequestScanner.GitHub.Services;

internal interface IGithubGraphQlClientProvider
{
    Connection GithubGraphQlClient { get; }
}