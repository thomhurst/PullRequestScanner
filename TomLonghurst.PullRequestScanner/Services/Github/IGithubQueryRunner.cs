using Octokit.GraphQL;

namespace TomLonghurst.PullRequestScanner.Services.Github;

internal interface IGithubQueryRunner
{
    Task<T> RunQuery<T>(ICompiledQuery<T> query);
}