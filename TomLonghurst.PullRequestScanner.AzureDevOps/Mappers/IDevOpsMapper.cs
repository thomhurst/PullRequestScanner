using TomLonghurst.PullRequestScanner.AzureDevOps.Models;
using TomLonghurst.PullRequestScanner.Models;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Mappers;

internal interface IDevOpsMapper
{
    PullRequest ToPullRequestModel(DevOpsPullRequestContext pullRequestContext);
}