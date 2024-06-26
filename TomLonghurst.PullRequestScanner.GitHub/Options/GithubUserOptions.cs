namespace TomLonghurst.PullRequestScanner.GitHub.Options;

public class GithubUserOptions : GithubOptions
{
    /**
     * <summary>Gets or sets the URL slug for the user.</summary>
     */
    public string Username { get; set; }

    internal override string CreateUriPathPrefix()
    {
        return $"users/{Username}/";
    }
}