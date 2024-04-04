namespace TomLonghurst.PullRequestScanner.Models;

public record Repository
{
    public string Name { get; set; }
    public string Id { get; set; }
    public string Url { get; set; }
}