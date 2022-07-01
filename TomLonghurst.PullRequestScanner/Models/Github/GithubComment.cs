namespace TomLonghurst.PullRequestScanner.Models.Github;

public record GithubComment
{
    public string Author { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
    public string Body { get; set; }
    public string Id { get; set; }
    public string Url { get; set; }
}