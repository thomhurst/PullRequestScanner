// <copyright file="IGithubRepositoryService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.GitHub.Services;

using TomLonghurst.PullRequestScanner.GitHub.Models;

internal interface IGithubRepositoryService
{
    Task<List<GithubRepository>> GetGitRepositories();
}