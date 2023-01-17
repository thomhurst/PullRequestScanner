namespace TomLonghurst.PullRequestScanner.AzureDevOps.Models;

internal class AzureDevOpsPullRequestContext
{
    public AzureDevOpsPullRequest AzureDevOpsPullRequest { get; set; }
    public IReadOnlyList<AzureDevOpsPullRequestThread> PullRequestThreads { get; set; }
    public IReadOnlyList<AzureDevOpsPullRequestIteration> Iterations { get; set; }
}