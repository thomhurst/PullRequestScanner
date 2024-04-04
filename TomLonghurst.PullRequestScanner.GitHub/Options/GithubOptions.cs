using TomLonghurst.PullRequestScanner.GitHub.Models;

namespace TomLonghurst.PullRequestScanner.GitHub.Options;

public abstract class GithubOptions
{
    public bool IsEnabled { get; set; } = true;
    /**
     * <summary>Personal Access Token, usually in the format of "{username}:{PAT}"</summary>
     */
    public string PersonalAccessToken { get; set; }

    public Func<GithubRepository, bool> RepositoriesToScan { get; set; } = _ => true;

    internal abstract string CreateUriPathPrefix();
}