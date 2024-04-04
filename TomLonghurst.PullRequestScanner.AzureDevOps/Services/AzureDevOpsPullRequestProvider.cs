// <copyright file="AzureDevOpsPullRequestProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

using System.Collections.Immutable;
using EnumerableAsyncProcessor.Extensions;
using TomLonghurst.PullRequestScanner.AzureDevOps.Mappers;
using TomLonghurst.PullRequestScanner.AzureDevOps.Options;
using TomLonghurst.PullRequestScanner.Contracts;
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
        this.devOpsGitRepositoryService = azureDevOpsGitRepositoryService;
        this.devOpsPullRequestService = azureDevOpsPullRequestService;
        this.devOpsMapper = azureDevOpsMapper;

        this.ValidateOptions();
    }

    public async Task<IReadOnlyList<PullRequest>> GetPullRequests()
    {
        if (this.azureDevOpsOptions?.IsEnabled != true)
        {
            return [];
        }

        var repositories = await this.devOpsGitRepositoryService.GetGitRepositories();

        var pullRequestsEnumerable = await repositories.ToAsyncProcessorBuilder()
            .SelectAsync(repo => this.devOpsPullRequestService.GetPullRequestsForRepository(repo))
            .ProcessInParallel(50, TimeSpan.FromSeconds(1));

        var azureDevOpsPullRequestContexts = pullRequestsEnumerable.SelectMany(x => x).ToImmutableList();

        var mappedPullRequests = azureDevOpsPullRequestContexts
            .Select(pr => this.devOpsMapper.ToPullRequestModel(pr))
            .ToImmutableList();

        return mappedPullRequests;
    }

    private void ValidateOptions()
    {
        if (this.azureDevOpsOptions.IsEnabled != true)
        {
            return;
        }

        ValidatePopulated(this.azureDevOpsOptions.Organization, nameof(this.azureDevOpsOptions.Organization));

        if (this.azureDevOpsOptions.ProjectGuid == default)
        {
            ValidatePopulated(this.azureDevOpsOptions.ProjectName, nameof(this.azureDevOpsOptions.ProjectName));
        }

        ValidatePopulated(this.azureDevOpsOptions.PersonalAccessToken, nameof(this.azureDevOpsOptions.PersonalAccessToken));

        static void ValidatePopulated(string value, string propertyName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(propertyName);
            }
        }
    }
}