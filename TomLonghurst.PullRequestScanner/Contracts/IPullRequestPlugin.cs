namespace TomLonghurst.PullRequestScanner.Contracts;

using TomLonghurst.PullRequestScanner.Models;

public interface IPullRequestPlugin
{
    Task ExecuteAsync(IReadOnlyList<PullRequest> pullRequests);
}