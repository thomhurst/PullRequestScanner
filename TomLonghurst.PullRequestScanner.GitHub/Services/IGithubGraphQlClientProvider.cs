// <copyright file="IGithubGraphQlClientProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.GitHub.Services;

using Octokit.GraphQL;

internal interface IGithubGraphQlClientProvider
{
    Connection GithubGraphQlClient { get; }
}