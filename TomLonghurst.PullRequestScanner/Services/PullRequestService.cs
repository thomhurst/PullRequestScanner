using System.Collections.Immutable;
using TomLonghurst.EnumerableAsyncProcessor.Extensions;
using TomLonghurst.PullRequestScanner.Mappers;
using TomLonghurst.PullRequestScanner.Models.Self;
using TomLonghurst.PullRequestScanner.Options;
using TomLonghurst.PullRequestScanner.Services.DevOps;
using TomLonghurst.PullRequestScanner.Services.Github;

namespace TomLonghurst.PullRequestScanner.Services;

internal class PullRequestService : IPullRequestService
{
    private readonly IDevOpsGitRepositoryService _devOpsGitRepositoryService;
    private readonly IDevOpsPullRequestService _devOpsPullRequestService;
    private readonly IDevOpsMapper _devOpsMapper;
    private readonly IGithubRepositoryService _githubRepositoryService;
    private readonly IGithubPullRequestService _githubPullRequestService;
    private readonly IGithubMapper _githubMapper;
    private readonly PullRequestScannerOptions _pullRequestScannerOptions;
    private readonly IEnumerable<IInitialize> _initializes;

    public PullRequestService(IDevOpsGitRepositoryService devOpsGitRepositoryService,
        IDevOpsPullRequestService devOpsPullRequestService,
        IDevOpsMapper devOpsMapper,
        IGithubRepositoryService githubRepositoryService,
        IGithubPullRequestService githubPullRequestService,
        IGithubMapper githubMapper,
        PullRequestScannerOptions pullRequestScannerOptions,
        IEnumerable<IInitialize> initializes)
    {
        _devOpsGitRepositoryService = devOpsGitRepositoryService;
        _devOpsPullRequestService = devOpsPullRequestService;
        _devOpsMapper = devOpsMapper;
        
        _githubRepositoryService = githubRepositoryService;
        _githubPullRequestService = githubPullRequestService;
        _githubMapper = githubMapper;
        _pullRequestScannerOptions = pullRequestScannerOptions;
        _initializes = initializes;
    }

    private void ValidatePopulated(string value, string propertyName)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(propertyName);
        }
    }

    public async Task<IReadOnlyList<PullRequest>> GetPullRequests()
    {
        ValidateOptions();
        await Task.WhenAll(_initializes.Select(x => x.Initialize()));
        var pullRequests = await Task.WhenAll(GetGithubPullRequests(), GetDevOpsPullRequests());
        return pullRequests.SelectMany(x => x).ToImmutableList();
    }

    private async Task<IReadOnlyList<PullRequest>> GetDevOpsPullRequests()
    {
        if (_pullRequestScannerOptions?.AzureDevOps?.IsEnabled != true)
        {
            return Array.Empty<PullRequest>();
        }
        
        var repositories = await _devOpsGitRepositoryService.GetGitRepositories();

        var pullRequestsEnumerable = await repositories.ToAsyncProcessorBuilder()
            .SelectAsync(repo => _devOpsPullRequestService.GetPullRequestsForRepository(repo))
            .ProcessInParallel(50, TimeSpan.FromSeconds(1));

        var devOpsPullRequestContexts = pullRequestsEnumerable.SelectMany(x => x).ToImmutableList();

        var mappedPullRequests = devOpsPullRequestContexts
            .Select(pr => _devOpsMapper.ToPullRequestModel(pr))
            .ToImmutableList();

        return mappedPullRequests;
    }

    private async Task<IReadOnlyList<PullRequest>> GetGithubPullRequests()
    {
        if (_pullRequestScannerOptions?.Github?.IsEnabled != true)
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
        var githubOptions = _pullRequestScannerOptions?.Github;
        if (githubOptions?.IsEnabled == true)
        {
            ValidateGithub(githubOptions);
        }

        var devOpsOptions = _pullRequestScannerOptions?.AzureDevOps;
        if (devOpsOptions?.IsEnabled == true)
        {
            ValidatePopulated(devOpsOptions.OrganizationSlug, nameof(devOpsOptions.OrganizationSlug));
            ValidatePopulated(devOpsOptions.TeamSlug, nameof(devOpsOptions.TeamSlug));
            ValidatePopulated(devOpsOptions.PersonalAccessToken, nameof(devOpsOptions.PersonalAccessToken));
        }
    }

    private void ValidateGithub(GithubOptions githubOptions)
    {
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
    }
}