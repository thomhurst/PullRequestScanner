// <copyright file="IGithubMapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.GitHub.Mappers;

using TomLonghurst.PullRequestScanner.GitHub.Models;
using TomLonghurst.PullRequestScanner.Models;

internal interface IGithubMapper
{
    PullRequest ToPullRequestModel(GithubPullRequest githubPullRequest);
}