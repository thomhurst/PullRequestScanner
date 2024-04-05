namespace TomLonghurst.PullRequestScanner.GitHub.Services;

using Models;

internal interface IGithubRepositoryService
{
    Task<List<GithubRepository>> GetGitRepositories();
}