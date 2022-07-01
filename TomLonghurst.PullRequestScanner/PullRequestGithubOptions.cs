namespace TomLonghurst.PullRequestScanner;

public class PullRequestGithubOptions
{
    public bool IsEnabled { get; set; } = true;
    public string OrganizationSlug { get; set; }
    public string TeamSlug { get; set; }
    public string PersonalAccessToken { get; set; }
}