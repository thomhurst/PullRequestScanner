// <copyright file="GithubRepositoryService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.GitHub.Services;

using TomLonghurst.PullRequestScanner.GitHub.Http;
using TomLonghurst.PullRequestScanner.GitHub.Models;
using TomLonghurst.PullRequestScanner.GitHub.Options;

internal class GithubRepositoryService : BaseGitHubApiService, IGithubRepositoryService
{
    private readonly GithubOptions githubOptions;

    public GithubRepositoryService(GithubHttpClient githubHttpClient, GithubOptions githubOptions)
        : base(githubHttpClient)
    {
        this.githubOptions = githubOptions;
    }

    public async Task<List<GithubRepository>> GetGitRepositories()
    {
        var gitRepositoryResponse = await this.Get<GithubRepository>(this.githubOptions.CreateUriPathPrefix() + "repos");

        return gitRepositoryResponse
            .Where(x => !x.Disabled)
            .Where(x => !x.Archived)
            .Where(x => this.githubOptions.RepositoriesToScan.Invoke(x))
            .ToList();
    }
}