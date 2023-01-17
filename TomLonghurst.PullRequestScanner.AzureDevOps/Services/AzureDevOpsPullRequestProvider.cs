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
    private readonly IDevOpsGitRepositoryService _devOpsGitRepositoryService;
    private readonly IDevOpsPullRequestService _devOpsPullRequestService;
    private readonly IDevOpsMapper _devOpsMapper;

    public AzureDevOpsPullRequestProvider(AzureDevOpsOptions azureDevOpsOptions,
        IDevOpsGitRepositoryService devOpsGitRepositoryService,
        IDevOpsPullRequestService devOpsPullRequestService,
        IDevOpsMapper devOpsMapper)
    {
        _azureDevOpsOptions = azureDevOpsOptions;
        _devOpsGitRepositoryService = devOpsGitRepositoryService;
        _devOpsPullRequestService = devOpsPullRequestService;
        _devOpsMapper = devOpsMapper;

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

        var devOpsPullRequestContexts = pullRequestsEnumerable.SelectMany(x => x).ToImmutableList();

        var mappedPullRequests = devOpsPullRequestContexts
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