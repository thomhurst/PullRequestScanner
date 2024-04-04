namespace TomLonghurst.PullRequestScanner.GitHub.Services;

using Http;
using Models;
using Options;

internal class GithubRepositoryService : BaseGitHubApiService, IGithubRepositoryService
{
    private readonly GithubOptions githubOptions;

    public GithubRepositoryService(GithubHttpClient githubHttpClient, GithubOptions githubOptions)
        : base(githubHttpClient)
    {
        this.githubOptions = githubOptions;
    }

    public async Task<List<GithubRepository>> GetGitRepositories()
    {
        var gitRepositoryResponse = await Get<GithubRepository>(githubOptions.CreateUriPathPrefix() + "repos");

        return gitRepositoryResponse
            .Where(x => !x.Disabled)
            .Where(x => !x.Archived)
            .Where(x => githubOptions.RepositoriesToScan.Invoke(x))
            .ToList();
    }
}