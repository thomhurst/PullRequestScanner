using TomLonghurst.PullRequestScanner.Models.Github;

namespace TomLonghurst.PullRequestScanner.Services.Github;

internal interface IGithubUserService
{
    Task<IReadOnlyList<GithubMember>> GetTeamMembers();
}