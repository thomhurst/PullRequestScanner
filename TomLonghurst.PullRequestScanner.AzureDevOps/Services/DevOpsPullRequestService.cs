using System.Collections.Immutable;
using TomLonghurst.PullRequestScanner.AzureDevOps.Http;
using TomLonghurst.PullRequestScanner.AzureDevOps.Models;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

internal class DevOpsPullRequestService : IDevOpsPullRequestService
{
    private readonly DevOpsHttpClient _devOpsHttpClient;

    public DevOpsPullRequestService(DevOpsHttpClient devOpsHttpClient)
    {
        _devOpsHttpClient = devOpsHttpClient;
    }
    
    public async Task<IReadOnlyList<DevOpsPullRequestContext>> GetPullRequestsForRepository(DevOpsGitRepository githubGitRepository)
    {
        var response = await _devOpsHttpClient.GetAll<DevOpsPullRequestsResponse>($"pullrequests?searchCriteria.status=all&searchCriteria.includeLinks=false&searchCriteria.repositoryId={githubGitRepository.Id}&includeCommits=true&api-version=7.1-preview.1");
        var nonDraftedPullRequests = response.SelectMany(x => x.PullRequests)
            .Where(IsActiveOrRecentlyClosed);

        var pullRequestsWithThreads = new List<DevOpsPullRequestContext>();

        foreach (var pullRequest in nonDraftedPullRequests)
        {
            var threads = await GetThreads(pullRequest);
            
            var iterations = await GetIterations(pullRequest);
            
            pullRequestsWithThreads.Add(new DevOpsPullRequestContext
            {
                DevOpsPullRequest = pullRequest,
                PullRequestThreads = threads,
                Iterations = iterations
            });
        }

        return pullRequestsWithThreads;
    }

    private bool IsActiveOrRecentlyClosed(DevOpsPullRequest devOpsPullRequest)
    {
        if (devOpsPullRequest.Status == "active")
        {
            return true;
        }

        if (devOpsPullRequest.ClosedDate >= DateTimeOffset.UtcNow.Date - TimeSpan.FromDays(1))
        {
            return true;
        }

        return false;
    }

    private async Task<IReadOnlyList<DevOpsPullRequestThread>> GetThreads(DevOpsPullRequest githubPullRequest)
    {
        var response = await _devOpsHttpClient.Get<DevOpsPullRequestThreadResponse>(
            $"repositories/{githubPullRequest.Repository.Id}/pullRequests/{githubPullRequest.PullRequestId}/threads?api-version=7.1-preview.1");

        return response.Threads.ToImmutableList();
    }

    private async Task<IReadOnlyList<DevOpsPullRequestIteration>> GetIterations(DevOpsPullRequest githubPullRequest)
    {
        var response = await _devOpsHttpClient.Get<DevOpsPullRequestIterationResponse>(
            $"repositories/{githubPullRequest.Repository.Id}/pullRequests/{githubPullRequest.PullRequestId}/statuses?api-version=7.1-preview.1");

        return response.Value.ToImmutableList();
    }
}