namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using TomLonghurst.PullRequestScanner.AzureDevOps.Models;
using TomLonghurst.PullRequestScanner.AzureDevOps.Options;

internal class AzureDevOpsPullRequestService(VssConnection vssConnection, AzureDevOpsOptions azureDevOpsOptions) : IAzureDevOpsPullRequestService
{
    public async Task<IReadOnlyList<AzureDevOpsPullRequestContext>> GetPullRequestsForRepository(
        GitRepository repository)
    {
        var pullRequests = new List<GitPullRequest>();

        var iteration = 0;
        do
        {
            var pullRequestsThisIteration = await vssConnection.GetClient<GitHttpClient>().GetPullRequestsAsync(
                project: azureDevOpsOptions.ProjectGuid,
                repositoryId: repository.Id,
                top: 100,
                skip: 100 * iteration,
                searchCriteria: new GitPullRequestSearchCriteria());

            pullRequests.AddRange(pullRequestsThisIteration);

            iteration++;
        }
        while (pullRequests.Count == 100 * iteration);

        var nonDraftedPullRequests = pullRequests.Where(IsActiveOrRecentlyClosed);

        var pullRequestsWithThreads = new List<AzureDevOpsPullRequestContext>();

        foreach (var pullRequest in nonDraftedPullRequests)
        {
            var threads = await GetThreads(pullRequest);

            var iterations = await GetStatuses(pullRequest);

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
        return await vssConnection.GetClient<GitHttpClient>().GetThreadsAsync(
            project: azureDevOpsOptions.ProjectGuid,
            repositoryId: pullRequest.Repository.Id,
            pullRequestId: pullRequest.PullRequestId);
    }

    private async Task<List<GitPullRequestStatus>> GetStatuses(GitPullRequest pullRequest)
    {
        return await vssConnection.GetClient<GitHttpClient>().GetPullRequestStatusesAsync(

            project: azureDevOpsOptions.ProjectGuid,
            repositoryId: pullRequest.Repository.Id,
            pullRequestId: pullRequest.PullRequestId);
    }
}