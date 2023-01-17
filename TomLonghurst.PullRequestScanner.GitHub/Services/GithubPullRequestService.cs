using Octokit.GraphQL;
using Octokit.GraphQL.Model;
using TomLonghurst.EnumerableAsyncProcessor.Extensions;
using TomLonghurst.PullRequestScanner.GitHub.Http;
using TomLonghurst.PullRequestScanner.GitHub.Models;

namespace TomLonghurst.PullRequestScanner.GitHub.Services;

internal class GithubPullRequestService : BaseGitHubApiService, IGithubPullRequestService
{
    private readonly IGithubQueryRunner _githubQueryRunner;

    public GithubPullRequestService(GithubHttpClient githubHttpClient, IGithubQueryRunner githubQueryRunner) : base(githubHttpClient)
    {
        _githubQueryRunner = githubQueryRunner;
    }

    public async Task<IEnumerable<GithubPullRequest>> GetPullRequests(GithubRepository repository)
    {
        var query = new Query()
            .Repository(owner: repository.Owner.Login, name: repository.Name)
            .PullRequests(states: new[] { PullRequestState.Open, PullRequestState.Closed, PullRequestState.Merged })
            .AllPages()
            .Select(x => new GithubPullRequest
            {
                Title = x.Title,
                Body = x.Body,
                Id = x.Id.Value,
                PullRequestNumber = x.Number,
                IsDraft = x.IsDraft,
                Mergeable = x.Mergeable,
                State = x.State,
                IsClosed = x.Closed,
                Created = x.CreatedAt,
                Author = x.Author.Login,
                ReviewDecision = x.ReviewDecision,
                LockReason = x.ActiveLockReason,
                LastUpdated = x.UpdatedAt,
                Url = x.Url,
                RepositoryId = x.Repository.Id.Value,
                RepositoryName = x.Repository.Name,
                RepositoryUrl = x.Repository.Url,
                ChecksStatus = x.Commits(null,null, 1, null)
                    .Nodes
                    .Select(x => 
                    x.Commit == null ? StatusState.Expected : 
                        x.Commit.StatusCheckRollup == null ? StatusState.Expected : x.Commit.StatusCheckRollup.State
                    )
                    .ToList()
                    .LastOrDefault(),
                Reviewers = x.Reviews(null, null, null, null, null, null)
                    .AllPages()
                    .Select(r => new GithubReviewer
                    {
                        Author = r.Author.Login,
                        State = r.State,
                        LastUpdated = r.LastEditedAt ?? r.PublishedAt ?? r.SubmittedAt ?? r.CreatedAt,
                        BodyText = r.BodyText,
                        Url = r.Url
                    }).ToList(),
            }).Compile();

        var pullRequests = (await _githubQueryRunner.RunQuery(query))
            .Where(IsActiveOrRecentlyClosed)
            .ToList();

        await pullRequests.ToAsyncProcessorBuilder()
            .ForEachAsync(async pr =>
            {
                var threadQuery = new Query()
                    .Repository(owner: repository.Owner.Login, name: repository.Name)
                    .PullRequest(pr.PullRequestNumber)
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

                pr.Threads = threads.ToList();
            })
            .ProcessInParallel(10, TimeSpan.FromSeconds(5));

        return pullRequests;
    }
    
    private bool IsActiveOrRecentlyClosed(GithubPullRequest githubPullRequest)
    {
        if (githubPullRequest.State == PullRequestState.Open)
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