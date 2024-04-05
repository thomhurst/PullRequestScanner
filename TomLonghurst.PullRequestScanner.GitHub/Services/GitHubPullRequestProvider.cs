namespace TomLonghurst.PullRequestScanner.GitHub.Services;

using System.Collections.Immutable;
using EnumerableAsyncProcessor.Extensions;
using Contracts;
using Mappers;
using Options;
using TomLonghurst.PullRequestScanner.Models;

internal class GitHubPullRequestProvider : IPullRequestProvider
{
    private readonly GithubOptions _githubOptions;
    private readonly IGithubRepositoryService _githubRepositoryService;
    private readonly IGithubPullRequestService _githubPullRequestService;
    private readonly IGithubMapper _githubMapper;

    public GitHubPullRequestProvider(
        GithubOptions githubOptions,
        IGithubRepositoryService githubRepositoryService,
        IGithubPullRequestService githubPullRequestService,
        IGithubMapper githubMapper)
    {
        _githubOptions = githubOptions;
        _githubRepositoryService = githubRepositoryService;
        _githubPullRequestService = githubPullRequestService;
        _githubMapper = githubMapper;

        ValidateOptions();
    }

    public async Task<IReadOnlyList<PullRequest>> GetPullRequests()
    {
        if (_githubOptions?.IsEnabled != true)
        {
            return Array.Empty<PullRequest>();
        }

        var repositories = await _githubRepositoryService.GetGitRepositories();

        var pullRequestsEnumerable = await repositories.ToAsyncProcessorBuilder()
            .SelectAsync(repo => _githubPullRequestService.GetPullRequests(repo))
            .ProcessInParallel(50, TimeSpan.FromSeconds(5));

        var pullRequests = pullRequestsEnumerable.SelectMany(x => x).ToImmutableList();

        var mappedPullRequests = pullRequests
            .Select(pr => _githubMapper.ToPullRequestModel(pr))
            .ToImmutableList();

        return mappedPullRequests;
    }

    private void ValidateOptions()
    {
        if (_githubOptions.IsEnabled != true)
        {
            return;
        }

        if (_githubOptions is GithubOrganizationTeamOptions githubOrganizationTeamOptions)
        {
            ValidatePopulated(githubOrganizationTeamOptions.OrganizationSlug, nameof(githubOrganizationTeamOptions.OrganizationSlug));
            ValidatePopulated(githubOrganizationTeamOptions.TeamSlug, nameof(githubOrganizationTeamOptions.TeamSlug));
        }

        if (_githubOptions is GithubUserOptions githubUserOptions)
        {
            ValidatePopulated(githubUserOptions.Username, nameof(githubUserOptions.Username));
        }

        ValidatePopulated(_githubOptions.PersonalAccessToken, nameof(_githubOptions.PersonalAccessToken));

        static void ValidatePopulated(string value, string propertyName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(propertyName);
            }
        }
    }
}