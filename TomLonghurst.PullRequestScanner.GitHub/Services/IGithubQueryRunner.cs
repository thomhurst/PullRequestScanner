using Octokit.GraphQL;

namespace TomLonghurst.PullRequestScanner.GitHub.Services;

internal interface IGithubQueryRunner
{
    Task<T> RunQuery<T>(ICompiledQuery<T> query);
}