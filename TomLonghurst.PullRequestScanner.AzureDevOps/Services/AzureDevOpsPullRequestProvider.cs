namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

using System.Collections.Immutable;
using EnumerableAsyncProcessor.Extensions;
using Mappers;
using Options;
using Contracts;
using TomLonghurst.PullRequestScanner.Models;

internal class AzureDevOpsPullRequestProvider : IPullRequestProvider
{
    private readonly AzureDevOpsOptions azureDevOpsOptions;
    private readonly IAzureDevOpsGitRepositoryService devOpsGitRepositoryService;
    private readonly IAzureDevOpsPullRequestService devOpsPullRequestService;
    private readonly IAzureDevOpsMapper devOpsMapper;

    public AzureDevOpsPullRequestProvider(
        AzureDevOpsOptions azureDevOpsOptions,
        IAzureDevOpsGitRepositoryService azureDevOpsGitRepositoryService,
        IAzureDevOpsPullRequestService azureDevOpsPullRequestService,
        IAzureDevOpsMapper azureDevOpsMapper)
    {
        this.azureDevOpsOptions = azureDevOpsOptions;
        devOpsGitRepositoryService = azureDevOpsGitRepositoryService;
        devOpsPullRequestService = azureDevOpsPullRequestService;
        devOpsMapper = azureDevOpsMapper;

        ValidateOptions();
    }

    public async Task<IReadOnlyList<PullRequest>> GetPullRequests()
    {
        if (azureDevOpsOptions?.IsEnabled != true)
        {
            return [];
        }

        var repositories = await devOpsGitRepositoryService.GetGitRepositories();

        var pullRequestsEnumerable = await repositories.ToAsyncProcessorBuilder()
            .SelectAsync(repo => devOpsPullRequestService.GetPullRequestsForRepository(repo))
            .ProcessInParallel(50, TimeSpan.FromSeconds(1));

        var azureDevOpsPullRequestContexts = pullRequestsEnumerable.SelectMany(x => x).ToImmutableList();

        var mappedPullRequests = azureDevOpsPullRequestContexts
            .Select(pr => devOpsMapper.ToPullRequestModel(pr))
            .ToImmutableList();

        return mappedPullRequests;
    }

    private void ValidateOptions()
    {
        if (azureDevOpsOptions.IsEnabled != true)
        {
            return;
        }

        ValidatePopulated(azureDevOpsOptions.Organization, nameof(azureDevOpsOptions.Organization));

        if (azureDevOpsOptions.ProjectGuid == default)
        {
            ValidatePopulated(azureDevOpsOptions.ProjectName, nameof(azureDevOpsOptions.ProjectName));
        }

        ValidatePopulated(azureDevOpsOptions.PersonalAccessToken, nameof(azureDevOpsOptions.PersonalAccessToken));

        static void ValidatePopulated(string value, string propertyName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(propertyName);
            }
        }
    }
}