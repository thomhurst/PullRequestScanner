namespace TomLonghurst.PullRequestScanner.Contracts;

using TomLonghurst.PullRequestScanner.Models;

public interface IPullRequestProvider
{
    Task<IReadOnlyList<PullRequest>> GetPullRequests();
}