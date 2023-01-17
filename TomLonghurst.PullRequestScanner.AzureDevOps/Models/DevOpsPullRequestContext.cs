namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

internal class DevOpsPullRequestContext
{
    public DevOpsPullRequest DevOpsPullRequest { get; set; }
    public IReadOnlyList<DevOpsPullRequestThread> PullRequestThreads { get; set; }
    public IReadOnlyList<DevOpsPullRequestIteration> Iterations { get; set; }
}