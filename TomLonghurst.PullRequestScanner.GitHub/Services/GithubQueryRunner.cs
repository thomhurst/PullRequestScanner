// <copyright file="GithubQueryRunner.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.GitHub.Services;

using System.Net;
using Octokit.GraphQL;
using Polly;

internal class GithubQueryRunner : IGithubQueryRunner
{
    private readonly IGithubGraphQlClientProvider githubGraphQlClientProvider;

    public GithubQueryRunner(IGithubGraphQlClientProvider githubGraphQlClientProvider)
    {
        this.githubGraphQlClientProvider = githubGraphQlClientProvider;
    }

    public async Task<T> RunQuery<T>(ICompiledQuery<T> query)
    {
        return await Policy.Handle<HttpRequestException>(this.ShouldHandleException)
            .Or<OperationCanceledException>()
            .WaitAndRetryAsync(5, i => TimeSpan.FromSeconds(i * 2))
            .ExecuteAsync(() => this.githubGraphQlClientProvider.GithubGraphQlClient.Run(query));
    }

    private bool ShouldHandleException(HttpRequestException httpRequestException)
    {
        if (httpRequestException.StatusCode is HttpStatusCode.RequestTimeout)
        {
            return true;
        }

        if ((int)httpRequestException.StatusCode >= 500)
        {
            return true;
        }

        return false;
    }
}