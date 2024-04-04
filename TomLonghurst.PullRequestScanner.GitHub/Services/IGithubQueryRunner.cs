// <copyright file="IGithubQueryRunner.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.GitHub.Services;

using Octokit.GraphQL;

internal interface IGithubQueryRunner
{
    Task<T> RunQuery<T>(ICompiledQuery<T> query);
}