using System.Collections.Immutable;
using TomLonghurst.EnumerableAsyncProcessor.Extensions;
using TomLonghurst.PullRequestScanner.AzureDevOps.Mappers;
using TomLonghurst.PullRequestScanner.AzureDevOps.Options;
using TomLonghurst.PullRequestScanner.Contracts;
using TomLonghurst.PullRequestScanner.Models;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

internal class AzureDevOpsPullRequestProvider : IPullRequestProvider
{
    private readonly AzureDevOpsOptions _azureDevOpsOptions;
    private readonly IAzureDevOpsGitRepositoryService _devOpsGitRepositoryService;
    private readonly IAzureDevOpsPullRequestService _devOpsPullRequestService;
    private readonly IAzureDevOpsMapper _devOpsMapper;

    public AzureDevOpsPullRequestProvider(AzureDevOpsOptions azureDevOpsOptions,
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
            return Array.Empty<PullRequest>();
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
        
        ValidatePopulated(_azureDevOpsOptions.OrganizationSlug, nameof(_azureDevOpsOptions.OrganizationSlug));
        ValidateCollectionPopulated(_azureDevOpsOptions.GetProjects(), nameof(_azureDevOpsOptions.ProjectSlug));
        ValidatePopulated(_azureDevOpsOptions.PersonalAccessToken, nameof(_azureDevOpsOptions.PersonalAccessToken));

        void ValidateCollectionPopulated(IEnumerable<string> value, string propertyName)
        {
            if (!value.Any())
            {
                throw new ArgumentNullException(propertyName);
            }
        }

        void ValidatePopulated(string value, string propertyName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(propertyName);
            }
        }
    }
}