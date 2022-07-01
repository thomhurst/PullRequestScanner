using TomLonghurst.PullRequestScanner.Models.Github;

namespace TomLonghurst.PullRequestScanner.Services.Github;

internal interface IGithubRepositoryService
{
    Task<List<GithubRepository>> GetGitRepositories();
}