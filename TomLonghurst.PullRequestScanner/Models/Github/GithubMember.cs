namespace TomLonghurst.PullRequestScanner.Models.Github;

internal class GithubMember
{
    public string DisplayName { get; set; }
    public string UniqueName { get; set; }
    public string Id { get; set; }
    public string Email { get; set; }
    public string ImageUrl { get; set; }
}