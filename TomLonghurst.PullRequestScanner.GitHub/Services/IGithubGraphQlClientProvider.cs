namespace TomLonghurst.PullRequestScanner.GitHub.Services;

using Octokit.GraphQL;

internal interface IGithubGraphQlClientProvider
{
    Connection GithubGraphQlClient { get; }
}