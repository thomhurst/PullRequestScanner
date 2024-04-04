namespace TomLonghurst.PullRequestScanner.GitHub.Models;

internal class GithubTeam
{
    public string Name { get; set; }

    public string Slug { get; set; }

    public string Id { get; set; }

    public List<GithubMember> Members { get; set; }
}