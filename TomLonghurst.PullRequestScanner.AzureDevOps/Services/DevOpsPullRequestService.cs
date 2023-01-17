using System.Collections.Immutable;
using TomLonghurst.PullRequestScanner.AzureDevOps.Http;
using TomLonghurst.PullRequestScanner.AzureDevOps.Models;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

internal class AzureDevOpsPullRequestService : IAzureDevOpsPullRequestService
{
    private readonly AzureDevOpsHttpClient _devOpsHttpClient;

    public AzureDevOpsPullRequestService(AzureDevOpsHttpClient azureDevOpsHttpClient)
    {
        _devOpsHttpClient = azureDevOpsHttpClient;
    }
    
    public async Task<IReadOnlyList<AzureDevOpsPullRequestContext>> GetPullRequestsForRepository(AzureDevOpsGitRepository githubGitRepository)
    {
        var response = await _devOpsHttpClient.GetAll<AzureDevOpsPullRequestsResponse>($"pullrequests?searchCriteria.status=all&searchCriteria.includeLinks=false&searchCriteria.repositoryId={githubGitRepository.Id}&includeCommits=true&api-version=7.1-preview.1");
        var nonDraftedPullRequests = response.SelectMany(x => x.PullRequests)
            .Where(IsActiveOrRecentlyClosed);

        var pullRequestsWithThreads = new List<AzureDevOpsPullRequestContext>();

        foreach (var pullRequest in nonDraftedPullRequests)
        {
            var threads = await GetThreads(pullRequest);
            
            var iterations = await GetIterations(pullRequest);
            
            pullRequestsWithThreads.Add(new AzureDevOpsPullRequestContext
            {
                AzureDevOpsPullRequest = pullRequest,
                PullRequestThreads = threads,
                Iterations = iterations
            });
        }

        return pullRequestsWithThreads;
    }

    private bool IsActiveOrRecentlyClosed(AzureDevOpsPullRequest azureDevOpsPullRequest)
    {
        if (azureDevOpsPullRequest.Status == "active")
        {
            return true;
        }

        if (azureDevOpsPullRequest.ClosedDate >= DateTimeOffset.UtcNow.Date - TimeSpan.FromDays(1))
        {
            return true;
        }

        return false;
    }

    private async Task<IReadOnlyList<AzureDevOpsPullRequestThread>> GetThreads(AzureDevOpsPullRequest githubPullRequest)
    {
        var response = await _devOpsHttpClient.Get<AzureDevOpsPullRequestThreadResponse>(
            $"repositories/{githubPullRequest.Repository.Id}/pullRequests/{githubPullRequest.PullRequestId}/threads?api-version=7.1-preview.1");

        return response.Threads.ToImmutableList();
    }

    private async Task<IReadOnlyList<AzureDevOpsPullRequestIteration>> GetIterations(AzureDevOpsPullRequest githubPullRequest)
    {
        var response = await _devOpsHttpClient.Get<AzureDevOpsPullRequestIterationResponse>(
            $"repositories/{githubPullRequest.Repository.Id}/pullRequests/{githubPullRequest.PullRequestId}/statuses?api-version=7.1-preview.1");

        return response.Value.ToImmutableList();
    }
}