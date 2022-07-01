using Octokit.GraphQL.Model;

namespace TomLonghurst.PullRequestScanner.Extensions;

public static class GraphQlExtensions
{
    public static StatusState GetStateSafely(this PullRequestCommitEdge? pullRequestCommitEdge)
    {
        return pullRequestCommitEdge?.Node?.Commit?.StatusCheckRollup?.State ?? StatusState.Expected;
    }
}