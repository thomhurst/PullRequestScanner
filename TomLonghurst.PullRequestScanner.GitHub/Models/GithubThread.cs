namespace TomLonghurst.PullRequestScanner.GitHub.Models;

public record GithubThread
{
    public List<GithubComment> Comments { get; set; }
    public bool IsResolved { get; set; }
    public string Url { get; set; }
}