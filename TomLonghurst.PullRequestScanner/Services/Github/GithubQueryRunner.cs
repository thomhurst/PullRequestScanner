using System.Net;
using Octokit.GraphQL;
using Polly;

namespace TomLonghurst.PullRequestScanner.Services.Github;

internal class GithubQueryRunner : IGithubQueryRunner
{
    private readonly IGithubGraphQlClientProvider _githubGraphQlClientProvider;

    public GithubQueryRunner(IGithubGraphQlClientProvider githubGraphQlClientProvider)
    {
        _githubGraphQlClientProvider = githubGraphQlClientProvider;
    }
    
    public async Task<T> RunQuery<T>(ICompiledQuery<T> query)
    {
        return await Policy.Handle<HttpRequestException>(ShouldHandleException)
            .Or<OperationCanceledException>()
            .WaitAndRetryAsync(5, i => TimeSpan.FromSeconds(i * 2))
            .ExecuteAsync(() => _githubGraphQlClientProvider.GithubGraphQlClient.Run(query));
    }

    private bool ShouldHandleException(HttpRequestException httpRequestException)
    {
        if (httpRequestException.StatusCode is HttpStatusCode.RequestTimeout)
        {
            return true;
        }

        if ((int) httpRequestException.StatusCode >= 500)
        {
            return true;
        }

        return false;
    }
}