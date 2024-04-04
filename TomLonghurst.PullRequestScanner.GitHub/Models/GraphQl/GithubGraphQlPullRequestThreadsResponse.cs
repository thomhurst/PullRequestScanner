﻿// <copyright file="GithubGraphQlPullRequestThreadsResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.GitHub.Models.GraphQl;

using System.Text.Json.Serialization;

public record GithubGraphQlPullRequestThreadsResponse(
    [property: JsonPropertyName("data")] Data Data);
