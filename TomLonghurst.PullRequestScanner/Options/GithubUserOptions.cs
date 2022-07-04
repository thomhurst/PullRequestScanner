namespace TomLonghurst.PullRequestScanner.Options;

public class GithubUserOptions : GithubOptions
{
    public string Username { get; set; }

    internal override string CreateUriPathPrefix()
    {
        return $"users/{Username}/";
    }
}