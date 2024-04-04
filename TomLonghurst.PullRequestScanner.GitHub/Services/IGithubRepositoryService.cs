using TomLonghurst.PullRequestScanner.GitHub.Models;

namespace TomLonghurst.PullRequestScanner.GitHub.Services;

internal interface IGithubRepositoryService
{
    Task<List<GithubRepository>> GetGitRepositories();
}