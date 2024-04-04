namespace TomLonghurst.PullRequestScanner.GitHub.Services;

using Http;
using Models;
using Options;

internal class GithubRepositoryService : BaseGitHubApiService, IGithubRepositoryService
{
    private readonly GithubOptions _githubOptions;

    public GithubRepositoryService(GithubHttpClient githubHttpClient, GithubOptions githubOptions)
        : base(githubHttpClient)
    {
        _githubOptions = githubOptions;
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