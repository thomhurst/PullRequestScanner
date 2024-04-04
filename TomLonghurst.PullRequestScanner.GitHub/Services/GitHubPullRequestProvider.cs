// <copyright file="GitHubPullRequestProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.GitHub.Services;

using System.Collections.Immutable;
using EnumerableAsyncProcessor.Extensions;
using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.GitHub.Mappers;
using TomLonghurst.PullRequestScanner.GitHub.Options;
using TomLonghurst.PullRequestScanner.Models;

internal class GitHubPullRequestProvider : IPullRequestProvider
{
    private readonly GithubOptions githubOptions;
    private readonly IGithubRepositoryService githubRepositoryService;
    private readonly IGithubPullRequestService githubPullRequestService;
    private readonly IGithubMapper githubMapper;

    public GitHubPullRequestProvider(
        GithubOptions githubOptions,
        IGithubRepositoryService githubRepositoryService,
        IGithubPullRequestService githubPullRequestService,
        IGithubMapper githubMapper)
    {
        this.githubOptions = githubOptions;
        this.githubRepositoryService = githubRepositoryService;
        this.githubPullRequestService = githubPullRequestService;
        this.githubMapper = githubMapper;

        this.ValidateOptions();
    }

    public async Task<IReadOnlyList<PullRequest>> GetPullRequests()
    {
        if (this.githubOptions?.IsEnabled != true)
        {
            return Array.Empty<PullRequest>();
        }

        var repositories = await this.githubRepositoryService.GetGitRepositories();

        var pullRequestsEnumerable = await repositories.ToAsyncProcessorBuilder()
            .SelectAsync(repo => this.githubPullRequestService.GetPullRequests(repo))
            .ProcessInParallel(50, TimeSpan.FromSeconds(5));

        var pullRequests = pullRequestsEnumerable.SelectMany(x => x).ToImmutableList();

        var mappedPullRequests = pullRequests
            .Select(pr => this.githubMapper.ToPullRequestModel(pr))
            .ToImmutableList();

        return mappedPullRequests;
    }

    private void ValidateOptions()
    {
        if (this.githubOptions.IsEnabled != true)
        {
            return;
        }

        if (this.githubOptions is GithubOrganizationTeamOptions githubOrganizationTeamOptions)
        {
            ValidatePopulated(githubOrganizationTeamOptions.OrganizationSlug, nameof(githubOrganizationTeamOptions.OrganizationSlug));
            ValidatePopulated(githubOrganizationTeamOptions.TeamSlug, nameof(githubOrganizationTeamOptions.TeamSlug));
        }

        if (this.githubOptions is GithubUserOptions githubUserOptions)
        {
            ValidatePopulated(githubUserOptions.Username, nameof(githubUserOptions.Username));
        }

        ValidatePopulated(this.githubOptions.PersonalAccessToken, nameof(this.githubOptions.PersonalAccessToken));

        static void ValidatePopulated(string value, string propertyName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(propertyName);
            }
        }
    }
}