using Octokit.GraphQL.Model;

namespace TomLonghurst.PullRequestScanner.Models.Github;

public record GithubPullRequest
{
    public string Title { get; set; }
    public LockReason? LockReason { get; set; }
    public int PullRequestNumber { get; set; }
    public string Id { get; set; }
    public bool IsDraft { get; set; }
    public MergeableState Mergeable { get; set; }
    public PullRequestState State { get; set; }
    public DateTimeOffset Created { get; set; }
    public string Author { get; set; }
    public PullRequestReviewDecision? ReviewDecision { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
    public string Url { get; set; }
    public string Body { get; set; }
    public string RepositoryId { get; set; }
    public string RepositoryName { get; set; }
    public string RepositoryUrl { get; set; }
    public List<GithubReviewer> Reviewers { get; set; }
    public List<GithubThread> Threads { get; set; }
    public StatusState ChecksStatus { get; set; }
}