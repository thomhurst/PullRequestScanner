namespace TomLonghurst.PullRequestScanner.AzureDevOps.Mappers;

using Models;
using TomLonghurst.PullRequestScanner.Models;

internal interface IAzureDevOpsMapper
{
    PullRequest ToPullRequestModel(AzureDevOpsPullRequestContext pullRequestContext);
}