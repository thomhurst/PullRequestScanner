// <copyright file="PullRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.GitHub.Models.GraphQl;

using System.Text.Json.Serialization;

public record PullRequest(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("reviewDecision")] string ReviewDecision,
    [property: JsonPropertyName("reviewThreads")] ReviewThreads ReviewThreads);