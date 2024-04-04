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

        ValidateOptions();
    }

    public async Task<IReadOnlyList<PullRequest>> GetPullRequests()
    {
        if (githubOptions?.IsEnabled != true)
        {
            return Array.Empty<PullRequest>();
        }

        var repositories = await githubRepositoryService.GetGitRepositories();

        var pullRequestsEnumerable = await repositories.ToAsyncProcessorBuilder()
            .SelectAsync(repo => githubPullRequestService.GetPullRequests(repo))
            .ProcessInParallel(50, TimeSpan.FromSeconds(5));

        var pullRequests = pullRequestsEnumerable.SelectMany(x => x).ToImmutableList();

        var mappedPullRequests = pullRequests
            .Select(pr => githubMapper.ToPullRequestModel(pr))
            .ToImmutableList();

        return mappedPullRequests;
    }

    private void ValidateOptions()
    {
        if (githubOptions.IsEnabled != true)
        {
            return;
        }

        if (githubOptions is GithubOrganizationTeamOptions githubOrganizationTeamOptions)
        {
            ValidatePopulated(githubOrganizationTeamOptions.OrganizationSlug, nameof(githubOrganizationTeamOptions.OrganizationSlug));
            ValidatePopulated(githubOrganizationTeamOptions.TeamSlug, nameof(githubOrganizationTeamOptions.TeamSlug));
        }

        if (githubOptions is GithubUserOptions githubUserOptions)
        {
            ValidatePopulated(githubUserOptions.Username, nameof(githubUserOptions.Username));
        }

        ValidatePopulated(githubOptions.PersonalAccessToken, nameof(githubOptions.PersonalAccessToken));

        static void ValidatePopulated(string value, string propertyName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(propertyName);
            }
        }
    }
}