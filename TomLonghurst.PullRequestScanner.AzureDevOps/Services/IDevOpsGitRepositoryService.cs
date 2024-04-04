// <copyright file="IDevOpsGitRepositoryService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Services;

using Microsoft.TeamFoundation.SourceControl.WebApi;

internal interface IAzureDevOpsGitRepositoryService
{
    Task<List<GitRepository>> GetGitRepositories();
}