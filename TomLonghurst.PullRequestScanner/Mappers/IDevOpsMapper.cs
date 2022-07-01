using TomLonghurst.PullRequestScanner.Models.DevOps;
using TomLonghurst.PullRequestScanner.Models.Self;

namespace TomLonghurst.PullRequestScanner.Mappers;

internal interface IDevOpsMapper
{
    PullRequest ToPullRequestModel(DevOpsPullRequestContext pullRequestContext);
}