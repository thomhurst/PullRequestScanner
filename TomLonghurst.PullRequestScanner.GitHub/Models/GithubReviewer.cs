namespace TomLonghurst.PullRequestScanner.GitHub.Models;

using PullRequestReviewState = Octokit.GraphQL.Model.PullRequestReviewState;

public record GithubReviewer
{
    public string Author { get; set; }

    public PullRequestReviewState State { get; set; }

    public DateTimeOffset LastUpdated { get; set; }

    public string BodyText { get; set; }

    public string Url { get; set; }
}