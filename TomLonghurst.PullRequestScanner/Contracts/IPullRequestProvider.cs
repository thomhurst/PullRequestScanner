namespace TomLonghurst.PullRequestScanner.Contracts;

using Models;

public interface IPullRequestProvider
{
    Task<IReadOnlyList<PullRequest>> GetPullRequests();
}