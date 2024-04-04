using TomLonghurst.PullRequestScanner.Models;

namespace TomLonghurst.PullRequestScanner.Contracts;

public interface IPullRequestProvider
{
    Task<IReadOnlyList<PullRequest>> GetPullRequests();
}