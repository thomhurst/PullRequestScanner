namespace TomLonghurst.PullRequestScanner.Models.Github;

public record GithubThread
{
    public List<GithubComment> Comments { get; set; }
    public bool IsResolved { get; set; }
    public string Url { get; set; }
}