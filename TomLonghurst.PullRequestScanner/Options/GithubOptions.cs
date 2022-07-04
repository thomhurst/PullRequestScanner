namespace TomLonghurst.PullRequestScanner.Options;

public abstract class GithubOptions
{
    public bool IsEnabled { get; set; } = true;
    public string PersonalAccessToken { get; set; }
    
    internal abstract string CreateUriPathPrefix();
}