using Octokit.GraphQL.Model;

namespace TomLonghurst.PullRequestScanner.Models.Github;

public record GithubReviewer
{
    public string Author { get; set; }
    public PullRequestReviewState State { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
    public string BodyText { get; set; }
    public string Url { get; set; }
}