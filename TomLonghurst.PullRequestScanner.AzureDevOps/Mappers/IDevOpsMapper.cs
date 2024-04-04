namespace TomLonghurst.PullRequestScanner.AzureDevOps.Mappers;

using TomLonghurst.PullRequestScanner.AzureDevOps.Models;
using TomLonghurst.PullRequestScanner.Models;

internal interface IAzureDevOpsMapper
{
    PullRequest ToPullRequestModel(AzureDevOpsPullRequestContext pullRequestContext);
}