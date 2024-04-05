namespace TomLonghurst.PullRequestScanner.GitHub.Services;

using Octokit.GraphQL;

internal interface IGithubQueryRunner
{
    Task<T> RunQuery<T>(ICompiledQuery<T> query);
}