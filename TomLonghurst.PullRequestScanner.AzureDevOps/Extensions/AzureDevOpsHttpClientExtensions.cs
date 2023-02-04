using System.Runtime.CompilerServices;
using TomLonghurst.PullRequestScanner.AzureDevOps.Http;
using TomLonghurst.PullRequestScanner.AzureDevOps.Models;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Extensions;

internal static class AzureDevOpsHttpClientExtensions
{
    public static async IAsyncEnumerable<AzureDevOpsPullRequest> GetPullRequests(this AzureDevOpsHttpClient azureDevOpsHttpClient, string repositoryId, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = await azureDevOpsHttpClient.GetAll<AzureDevOpsPullRequestsResponse>(
            $"_apis/git/pullrequests?searchCriteria.status=all&searchCriteria.includeLinks=false&searchCriteria.repositoryId={repositoryId}&includeCommits=true&api-version=7.1-preview.1", cancellationToken)
            .ConfigureAwait(false);

        foreach (var azureDevOpsPullRequestsResponse in response)
        {
            foreach (AzureDevOpsPullRequest azureDevOpsPullRequest in azureDevOpsPullRequestsResponse.PullRequests)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    yield break;
                }

                yield return azureDevOpsPullRequest;
            }
        }
    }

    public static async IAsyncEnumerable<AzureDevOpsPullRequestIteration> ListPullRequestIterations(this AzureDevOpsHttpClient azureDevOpsHttpClient, string repositoryId, int pullRequestId, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = await azureDevOpsHttpClient.Get<AzureDevOpsPullRequestIterationResponse>(
            $"_apis/git/repositories/{repositoryId}/pullRequests/{pullRequestId}/statuses?api-version=7.1-preview.1", cancellationToken).ConfigureAwait(false);

        if (response is null)
        {
            yield break;
        }

        foreach (var azureDevOpsPullRequestIteration in response.Value)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            yield return azureDevOpsPullRequestIteration;
        }
    }

    public static async IAsyncEnumerable<AzureDevOpsPullRequestThread> ListPullRequestThreads(this AzureDevOpsHttpClient azureDevOpsHttpClient, string repositoryId, int pullRequestId, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = await azureDevOpsHttpClient.Get<AzureDevOpsPullRequestThreadResponse>(
            $"_apis/git/repositories/{repositoryId}/pullRequests/{pullRequestId}/threads?api-version=7.1-preview.1", cancellationToken).ConfigureAwait(false);

        if (response is null)
        {
            yield break;
        }

        foreach (AzureDevOpsPullRequestThread azureDevOpsPullRequestThread in response.Threads)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            yield return azureDevOpsPullRequestThread;
        }
    }
    
    public static async IAsyncEnumerable<AzureDevOpsTeamMember> ListTeamMembers(
        this AzureDevOpsHttpClient azureDevOpsHttpClient, string project, string teamId, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = await azureDevOpsHttpClient.GetAll<AzureDevOpsTeamMembersResponseWrapper>(
            $"_apis/projects/{project}/teams/{teamId}/members?api-version=7.1-preview.2", cancellationToken)
            .ConfigureAwait(false);


        foreach (var azureDevOpsTeamMembersResponseWrapper in response)
        {
            foreach (AzureDevOpsTeamMember azureDevOpsTeamMember in azureDevOpsTeamMembersResponseWrapper.Value)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    yield break;
                }


                yield return azureDevOpsTeamMember;
            }

            
        }
    }

    public static async IAsyncEnumerable<AzureDevOpsTeam> ListTeams(
        this AzureDevOpsHttpClient azureDevOpsHttpClient, string project, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = await azureDevOpsHttpClient
            .GetAll<AzureDevOpsTeamWrapper>($"_apis/projects/{project}/teams?api-version=7.1-preview.2", cancellationToken)
            .ConfigureAwait(false);

        foreach (AzureDevOpsTeamWrapper azureDevOpsTeamWrapper in response)
        {
            foreach (AzureDevOpsTeam azureDevOpsTeam in azureDevOpsTeamWrapper.Value)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    yield break;
                }

                yield return azureDevOpsTeam;
            }
        }
    }

    public static async IAsyncEnumerable<AzureDevOpsGitRepository> ListGitRepositories(
        this AzureDevOpsHttpClient azureDevOpsHttpClient, string project, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = await azureDevOpsHttpClient
            .GetAll<AzureDevOpsGitRepositoryResponse>($"{project}/_apis/git/repositories?api-version=7.1-preview.1", cancellationToken)
            .ConfigureAwait(false);

        foreach (AzureDevOpsGitRepositoryResponse azureDevOpsGitRepositoryResponse in response)
        {
            foreach (AzureDevOpsGitRepository azureDevOpsGitRepository in azureDevOpsGitRepositoryResponse.Repositories)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    yield break;
                }

                yield return azureDevOpsGitRepository;
            }
        }
    }
}