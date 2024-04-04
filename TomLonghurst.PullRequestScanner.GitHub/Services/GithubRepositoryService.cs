namespace TomLonghurst.PullRequestScanner.GitHub.Services;

using TomLonghurst.PullRequestScanner.GitHub.Http;
using TomLonghurst.PullRequestScanner.GitHub.Models;
using TomLonghurst.PullRequestScanner.GitHub.Options;

internal class GithubRepositoryService : BaseGitHubApiService, IGithubRepositoryService
{
    private readonly GithubOptions _githubOptions;

    public GithubRepositoryService(GithubHttpClient githubHttpClient, GithubOptions githubOptions)
        : base(githubHttpClient)
    {
        this._githubOptions = githubOptions;
    }

    public async Task<List<GithubRepository>> GetGitRepositories()
    {
        var gitRepositoryResponse = await Get<GithubRepository>(_githubOptions.CreateUriPathPrefix() + "repos");

        return gitRepositoryResponse
            .Where(x => !x.Disabled)
            .Where(x => !x.Archived)
            .Where(x => _githubOptions.RepositoriesToScan.Invoke(x))
            .ToList();
    }
}