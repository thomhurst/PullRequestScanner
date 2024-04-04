// <copyright file="DevOpsPullRequestService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using TomLonghurst.PullRequestScanner.AzureDevOps.Models;
using TomLonghurst.PullRequestScanner.AzureDevOps.Options;

internal class AzureDevOpsPullRequestService(VssConnection vssConnection, AzureDevOpsOptions azureDevOpsOptions) : IAzureDevOpsPullRequestService
{
    private readonly VssConnection vssConnection = vssConnection;
    private readonly AzureDevOpsOptions azureDevOpsOptions = azureDevOpsOptions;

    public async Task<IReadOnlyList<AzureDevOpsPullRequestContext>> GetPullRequestsForRepository(
        GitRepository repository)
    {
        var pullRequests = new List<GitPullRequest>();

        var iteration = 0;
        do
        {
            var pullRequestsThisIteration = await this.vssConnection.GetClient<GitHttpClient>().GetPullRequestsAsync(
                project: this.azureDevOpsOptions.ProjectGuid,
                repositoryId: repository.Id,
                top: 100,
                skip: 100 * iteration,
                searchCriteria: new GitPullRequestSearchCriteria());

            pullRequests.AddRange(pullRequestsThisIteration);

            iteration++;
        }
        while (pullRequests.Count == 100 * iteration);

        var nonDraftedPullRequests = pullRequests.Where(this.IsActiveOrRecentlyClosed);

        var pullRequestsWithThreads = new List<AzureDevOpsPullRequestContext>();

        foreach (var pullRequest in nonDraftedPullRequests)
        {
            var threads = await this.GetThreads(pullRequest);

            var iterations = await this.GetStatuses(pullRequest);

            pullRequestsWithThreads.Add(new AzureDevOpsPullRequestContext
            {
                AzureDevOpsPullRequest = pullRequest,
                PullRequestThreads = threads,
                Iterations = iterations,
            });
        }

        return pullRequestsWithThreads;
    }

    private bool IsActiveOrRecentlyClosed(GitPullRequest gitPullRequest)
    {
        if (gitPullRequest.Status == PullRequestStatus.Active)
        {
            return true;
        }

        if (gitPullRequest.ClosedDate >= DateTimeOffset.UtcNow.Date - TimeSpan.FromDays(1))
        {
            return true;
        }

        return false;
    }

    private async Task<List<GitPullRequestCommentThread>> GetThreads(GitPullRequest pullRequest)
    {
        return await this.vssConnection.GetClient<GitHttpClient>().GetThreadsAsync(
            project: this.azureDevOpsOptions.ProjectGuid,
            repositoryId: pullRequest.Repository.Id,
            pullRequestId: pullRequest.PullRequestId);
    }

    private async Task<List<GitPullRequestStatus>> GetStatuses(GitPullRequest pullRequest)
    {
        return await this.vssConnection.GetClient<GitHttpClient>().GetPullRequestStatusesAsync(

            project: this.azureDevOpsOptions.ProjectGuid,
            repositoryId: pullRequest.Repository.Id,
            pullRequestId: pullRequest.PullRequestId);
    }
}