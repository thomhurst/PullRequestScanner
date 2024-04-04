// <copyright file="Comments.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.GitHub.Models.GraphQl;

using System.Text.Json.Serialization;

public record Comments(
    [property: JsonPropertyName("totalCount")] int TotalCount,
    [property: JsonPropertyName("nodes")] IReadOnlyList<CommentNode> Nodes);