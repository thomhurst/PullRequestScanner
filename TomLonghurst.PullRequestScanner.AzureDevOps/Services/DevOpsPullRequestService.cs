using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using TomLonghurst.PullRequestScanner.AzureDevOps.Models;
using TomLonghurst.PullRequestScanner.AzureDevOps.Options;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

internal class AzureDevOpsPullRequestService(VssConnection vssConnection, AzureDevOpsOptions azureDevOpsOptions) : IAzureDevOpsPullRequestService
{
    private readonly VssConnection _vssConnection = vssConnection;
    private readonly AzureDevOpsOptions _azureDevOpsOptions = azureDevOpsOptions;

    public async Task<IReadOnlyList<AzureDevOpsPullRequestContext>> GetPullRequestsForRepository(
        GitRepository repository)
    {
        var pullRequests = new List<GitPullRequest>();

        var iteration = 0;
        do
        {
            var pullRequestsThisIteration = await _vssConnection.GetClient<GitHttpClient>().GetPullRequestsAsync(
                project: _azureDevOpsOptions.ProjectGuid,
                repositoryId: repository.Id,
                top: 100,
                skip: 100 * iteration,
                searchCriteria: new GitPullRequestSearchCriteria());

            pullRequests.AddRange(pullRequestsThisIteration);

            iteration++;
        } while (pullRequests.Count == 100 * iteration);


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
                Iterations = iterations
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
        return await _vssConnection.GetClient<GitHttpClient>().GetThreadsAsync(
            project: _azureDevOpsOptions.ProjectGuid,
            repositoryId: pullRequest.Repository.Id,
            pullRequestId: pullRequest.PullRequestId
        );
    }

    private async Task<List<GitPullRequestStatus>> GetStatuses(GitPullRequest pullRequest)
    {
        return await _vssConnection.GetClient<GitHttpClient>().GetPullRequestStatusesAsync(

            project: _azureDevOpsOptions.ProjectGuid,
            repositoryId: pullRequest.Repository.Id,
            pullRequestId: pullRequest.PullRequestId
        );
    }
}