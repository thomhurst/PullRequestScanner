namespace TomLonghurst.PullRequestScanner.Contracts;

using Models;

public interface IPullRequestPlugin
{
    Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests);
}