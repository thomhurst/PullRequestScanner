using EnumerableAsyncProcessor.Extensions;
using Octokit;
using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using TomLonghurst.PullRequestScanner.GitHub.Http;
using TomLonghurst.PullRequestScanner.GitHub.Models;
using MergeableState = Octokit.MergeableState;
using PullRequest = Octokit.PullRequest;

namespace TomLonghurst.PullRequestScanner.GitHub.Services;

internal class GithubPullRequestService : BaseGitHubApiService, IGithubPullRequestService
{
    private readonly IGitHubClient _gitHubClient;
    private readonly IGithubQueryRunner _githubQueryRunner;

    public GithubPullRequestService(GithubHttpClient githubHttpClient, IGitHubClient gitHubClient, IGithubQueryRunner githubQueryRunner) : base(githubHttpClient)
    {
        _gitHubClient = gitHubClient;
        _githubQueryRunner = githubQueryRunner;
    }

    public async Task<IEnumerable<GithubPullRequest>> GetPullRequests(GithubRepository repository)
    {
        var pullRequests = await _gitHubClient.PullRequest
            .GetAllForRepository(owner: repository.Owner.Login, name: repository.Name);

        var mapped = await pullRequests
            .SelectAsync(async x => new GithubPullRequest
            {
                Title = x.Title,
                Body = x.Body,
                Id = x.Id.ToString(),
                PullRequestNumber = x.Number,
                IsDraft = x.Draft,
                IsMerged = x.Merged,
                Mergeable = x.MergeableState?.Value ?? MergeableState.Unknown,
                State = x.State.Value,
                IsClosed = x.ClosedAt != null,
                Created = x.CreatedAt,
                Author = x.User.Login,
                ReviewDecision = await GetReviewDecision(repository, x),
                LockReason = x.ActiveLockReason?.Value,
                LastUpdated = x.UpdatedAt,
                Url = x.Url,
                RepositoryId = repository.Id.ToString(),
                RepositoryName = repository.Name,
                RepositoryUrl = repository.Url,
                ChecksStatus = await GetLastCommitCheckStatus(repository, x),
                Reviewers = await GetReviewers(repository, x),
                Labels = x.Labels.Select(x => x.Name).ToList(),
                Threads = await GetThreads(repository, x)
            })
            .ProcessInParallel();
            
            return mapped
            .Where(IsActiveOrRecentlyClosed);
    }

    private async Task<List<GithubReviewer>> GetReviewers(GithubRepository repository, PullRequest pullRequest)
    {
        var query = new Query()
            .Repository(owner: repository.Owner.Login, name: repository.Name)
            .PullRequest(pullRequest.Number)
            .Reviews(null, null, null, null, null, null)
            .AllPages()
            .Select(r => new GithubReviewer
            {
                Author = r.Author.Login,
                State = r.State,
                LastUpdated = r.LastEditedAt ?? r.PublishedAt ?? r.SubmittedAt ?? r.CreatedAt,
                BodyText = r.BodyText,
                Url = r.Url
            })
            .Compile();

        var reviewers = await _githubQueryRunner.RunQuery(query);
        
        return reviewers.ToList();
    }

    private async Task<List<GithubThread>> GetThreads(GithubRepository repository, PullRequest pullRequest)
    {
        var threadQuery = new Query()
            .Repository(owner: repository.Owner.Login, name: repository.Name)
            .PullRequest(pullRequest.Number)
            .ReviewThreads()
            .AllPages()
            .Select(t => new GithubThread
            {
                IsResolved = t.IsResolved,
                Comments = t.Comments(null, null, null, null, null)
                    .AllPages()
                    .Select(c =>
                        new GithubComment
                        {
                            Author = c.Author.Login,
                            LastUpdated = c.UpdatedAt,
                            Body = c.Body,
                            Id = c.Id.Value,
                            Url = c.Url
                        }).ToList()
            }).Compile();

        var threads = await _githubQueryRunner.RunQuery(threadQuery);
        
        return threads.ToList();
    }

    private async Task<StatusState> GetLastCommitCheckStatus(GithubRepository repository, PullRequest pullRequest)
    {
        var query = new Query()
            .Repository(owner: repository.Owner.Login, name: repository.Name)
            .PullRequest(pullRequest.Number)
            .Commits(null, null, 1, null)
            .Nodes
            .Select(x =>
                x.Commit == null ? StatusState.Expected :
                x.Commit.StatusCheckRollup == null ? StatusState.Expected : x.Commit.StatusCheckRollup.State
            ).Compile();

        var statuses = await _githubQueryRunner.RunQuery(query);

        return statuses.LastOrDefault();
    }

    private async Task<PullRequestReviewDecision?> GetReviewDecision(GithubRepository repository, PullRequest pullRequest)
    {
        var query = new Query()
            .Repository(owner: repository.Owner.Login, name: repository.Name)
            .PullRequest(pullRequest.Number)
            .Select(x => x.ReviewDecision)
            .Compile();

        var reviewDecision = await _githubQueryRunner.RunQuery(query);

        return reviewDecision;
    }

    private bool IsActiveOrRecentlyClosed(GithubPullRequest githubPullRequest)
    {
        if (githubPullRequest.State == ItemState.Open)
        {
            return true;
        }

        if (githubPullRequest.LastUpdated >= DateTimeOffset.UtcNow.Date - TimeSpan.FromDays(1))
        {
            return true;
        }

        return false;
    }
}