namespace TomLonghurst.PullRequestScanner.GitHub.Services;

using TomLonghurst.PullRequestScanner.GitHub.Models;

internal interface IGithubRepositoryService
{
    Task<List<GithubRepository>> GetGitRepositories();
}