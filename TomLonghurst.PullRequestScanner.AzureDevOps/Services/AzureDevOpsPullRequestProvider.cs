namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

using System.Collections.Immutable;
using EnumerableAsyncProcessor.Extensions;
using Mappers;
using Options;
using Contracts;
using TomLonghurst.PullRequestScanner.Models;

internal class AzureDevOpsPullRequestProvider : IPullRequestProvider
{
    private readonly AzureDevOpsOptions _azureDevOpsOptions;
    private readonly IAzureDevOpsGitRepositoryService _devOpsGitRepositoryService;
    private readonly IAzureDevOpsPullRequestService _devOpsPullRequestService;
    private readonly IAzureDevOpsMapper _devOpsMapper;

    public AzureDevOpsPullRequestProvider(
        AzureDevOpsOptions azureDevOpsOptions,
        IAzureDevOpsGitRepositoryService azureDevOpsGitRepositoryService,
        IAzureDevOpsPullRequestService azureDevOpsPullRequestService,
        IAzureDevOpsMapper azureDevOpsMapper)
    {
        _azureDevOpsOptions = azureDevOpsOptions;
        _devOpsGitRepositoryService = azureDevOpsGitRepositoryService;
        _devOpsPullRequestService = azureDevOpsPullRequestService;
        _devOpsMapper = azureDevOpsMapper;

        ValidateOptions();
    }

    public async Task<IReadOnlyList<PullRequest>> GetPullRequests()
    {
        if (_azureDevOpsOptions?.IsEnabled != true)
        {
            return [];
        }

        var repositories = await _devOpsGitRepositoryService.GetGitRepositories();

        var pullRequestsEnumerable = await repositories.ToAsyncProcessorBuilder()
            .SelectAsync(repo => _devOpsPullRequestService.GetPullRequestsForRepository(repo))
            .ProcessInParallel(50, TimeSpan.FromSeconds(1));

        var azureDevOpsPullRequestContexts = pullRequestsEnumerable.SelectMany(x => x).ToImmutableList();

        var mappedPullRequests = azureDevOpsPullRequestContexts
            .Select(pr => _devOpsMapper.ToPullRequestModel(pr))
            .ToImmutableList();

        return mappedPullRequests;
    }

    private void ValidateOptions()
    {
        if (_azureDevOpsOptions.IsEnabled != true)
        {
            return;
        }

        ValidatePopulated(_azureDevOpsOptions.Organization, nameof(_azureDevOpsOptions.Organization));

        if (_azureDevOpsOptions.ProjectGuid == default)
        {
            ValidatePopulated(_azureDevOpsOptions.ProjectName, nameof(_azureDevOpsOptions.ProjectName));
        }

        ValidatePopulated(_azureDevOpsOptions.PersonalAccessToken, nameof(_azureDevOpsOptions.PersonalAccessToken));

        static void ValidatePopulated(string value, string propertyName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(propertyName);
            }
        }
    }
}