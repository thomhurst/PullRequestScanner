namespace TomLonghurst.PullRequestScanner.Options;

public abstract class GithubOptions
{
    public bool IsEnabled { get; set; } = true;
    /**
     * <summary>Personal Access Token, usually in the format of "{username}:{PAT}"</summary>
     */
    public string PersonalAccessToken { get; set; }
    
    internal abstract string CreateUriPathPrefix();
}