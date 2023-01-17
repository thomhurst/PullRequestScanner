namespace TomLonghurst.PullRequestScanner.GitHub.Options;

public class GithubOrganizationTeamOptions : GithubOptions
{
    /**
     * <summary>The URL slug for the organization</summary>
     */
    public string OrganizationSlug { get; set; }
    /**
     * <summary>The URL slug for the team</summary>
     */
    public string TeamSlug { get; set; }

    internal override string CreateUriPathPrefix()
    {
        return $"orgs/{OrganizationSlug}/teams/{TeamSlug}/";
    }
}