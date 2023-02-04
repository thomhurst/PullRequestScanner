using System.Collections.Immutable;
using TomLonghurst.PullRequestScanner.AzureDevOps.Extensions;
using TomLonghurst.PullRequestScanner.AzureDevOps.Http;
using TomLonghurst.PullRequestScanner.AzureDevOps.Models;
using TomLonghurst.PullRequestScanner.AzureDevOps.Options;
using TomLonghurst.PullRequestScanner.Extensions;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

internal class AzureDevOpsPullRequestService : IAzureDevOpsPullRequestService
{
    private readonly AzureDevOpsHttpClient _devOpsHttpClient;

    public AzureDevOpsPullRequestService(AzureDevOpsHttpClient azureDevOpsHttpClient, AzureDevOpsOptions azureDevOpsOptions)
    {
        _devOpsHttpClient = azureDevOpsHttpClient;
    }
    
    public async Task<IReadOnlyList<AzureDevOpsPullRequestContext>> GetPullRequestsForRepository(AzureDevOpsGitRepository githubGitRepository)
    {
        var pullRequests = await _devOpsHttpClient
            .GetPullRequests(githubGitRepository.Id)
            .ToImmutableList();
        
        var nonDraftedPullRequests = pullRequests
            .Where(IsActiveOrRecentlyClosed);

        var pullRequestsWithThreads = new List<AzureDevOpsPullRequestContext>();

        foreach (var pullRequest in nonDraftedPullRequests)
        {
            var threads =
                await _devOpsHttpClient
                    .ListPullRequestThreads(pullRequest.Repository.Id, pullRequest.PullRequestId)
                    .ToImmutableList();

            var iterations = await _devOpsHttpClient
                .ListPullRequestIterations(pullRequest.Repository.Id, pullRequest.PullRequestId)
                .ToImmutableList();

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
}