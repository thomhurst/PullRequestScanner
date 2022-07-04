namespace TomLonghurst.PullRequestScanner.Options;

public class GithubOrganizationTeamOptions : GithubOptions
{
    public string OrganizationSlug { get; set; }
    public string TeamSlug { get; set; }

    internal override string CreateUriPathPrefix()
    {
        return $"orgs/{OrganizationSlug}/teams/{TeamSlug}/";
    }
}