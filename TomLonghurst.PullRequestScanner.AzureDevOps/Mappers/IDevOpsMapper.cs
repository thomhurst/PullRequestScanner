using TomLonghurst.PullRequestScanner.AzureDevOps.Models;
using TomLonghurst.PullRequestScanner.Models;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Mappers;

internal interface IAzureDevOpsMapper
{
    PullRequest ToPullRequestModel(AzureDevOpsPullRequestContext pullRequestContext);
}