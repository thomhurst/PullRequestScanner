namespace TomLonghurst.PullRequestScanner.Options;

public class GithubUserOptions : GithubOptions
{
    /**
     * <summary>The URL slug for the user</summary>
     */
    public string Username { get; set; }

    internal override string CreateUriPathPrefix()
    {
        return $"users/{Username}/";
    }
}