// <copyright file="IDevOpsMapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Mappers;

using TomLonghurst.PullRequestScanner.AzureDevOps.Models;
using TomLonghurst.PullRequestScanner.Models;

internal interface IAzureDevOpsMapper
{
    PullRequest ToPullRequestModel(AzureDevOpsPullRequestContext pullRequestContext);
}