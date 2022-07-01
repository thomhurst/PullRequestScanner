using TomLonghurst.PullRequestScanner.Http;
using TomLonghurst.PullRequestScanner.Models.Github;

namespace TomLonghurst.PullRequestScanner.Services.Github;

internal class GithubRepositoryService : BaseGitHubApiService, IGithubRepositoryService
{
    public GithubRepositoryService(GithubHttpClient githubHttpClient) : base(githubHttpClient)
    {
    }

    public async Task<List<GithubRepository>> GetGitRepositories()
    {

        var gitRepositoryResponse =
            await Get<GithubRepository>("orgs/asosteam/teams/customer-profile-identity/repos");

        return gitRepositoryResponse
            .Where(x => !x.Disabled)
            .Where(x => !x.Archived)
            .ToList();
    }
}