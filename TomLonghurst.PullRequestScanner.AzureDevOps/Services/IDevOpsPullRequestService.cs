// <copyright file="IDevOpsPullRequestService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

using Microsoft.TeamFoundation.SourceControl.WebApi;
using TomLonghurst.PullRequestScanner.AzureDevOps.Models;

internal interface IAzureDevOpsPullRequestService
{
    Task<IReadOnlyList<AzureDevOpsPullRequestContext>> GetPullRequestsForRepository(GitRepository repository);
}