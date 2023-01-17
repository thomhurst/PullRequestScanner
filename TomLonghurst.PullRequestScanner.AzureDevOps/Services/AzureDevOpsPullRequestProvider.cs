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
        IAzureDevOpsGitRepositoryService AzureDevOpsGitRepositoryService,
        IAzureDevOpsPullRequestService AzureDevOpsPullRequestService,
        IAzureDevOpsMapper AzureDevOpsMapper)
    {
        _azureDevOpsOptions = azureDevOpsOptions;
        _devOpsGitRepositoryService = AzureDevOpsGitRepositoryService;
        _devOpsPullRequestService = AzureDevOpsPullRequestService;
        _devOpsMapper = AzureDevOpsMapper;

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

        var AzureDevOpsPullRequestContexts = pullRequestsEnumerable.SelectMany(x => x).ToImmutableList();

        var mappedPullRequests = AzureDevOpsPullRequestContexts
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
        ValidatePopulated(_azureDevOpsOptions.ProjectSlug, nameof(_azureDevOpsOptions.ProjectSlug));
        ValidatePopulated(_azureDevOpsOptions.PersonalAccessToken, nameof(_azureDevOpsOptions.PersonalAccessToken));
        
        void ValidatePopulated(string value, string propertyName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(propertyName);
            }
        }
    }
}