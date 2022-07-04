using TomLonghurst.PullRequestScanner.Http;
using TomLonghurst.PullRequestScanner.Models.Github;
using TomLonghurst.PullRequestScanner.Options;

namespace TomLonghurst.PullRequestScanner.Services.Github;

internal class GithubRepositoryService : BaseGitHubApiService, IGithubRepositoryService
{
    private readonly PullRequestScannerOptions _pullRequestScannerOptions;
    private readonly GithubOptions _githubOptions;

    public GithubRepositoryService(GithubHttpClient githubHttpClient, PullRequestScannerOptions pullRequestScannerOptions) : base(githubHttpClient)
    {
        _pullRequestScannerOptions = pullRequestScannerOptions;
    }

    public async Task<List<GithubRepository>> GetGitRepositories()
    {
        var gitRepositoryResponse = await Get<GithubRepository>(_pullRequestScannerOptions.Github.CreateUriPathPrefix() + "repos");

        return gitRepositoryResponse
            .Where(x => !x.Disabled)
            .Where(x => !x.Archived)
            .ToList();
    }
}