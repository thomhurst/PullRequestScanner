// <copyright file="CommentThreadNode.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.GitHub.Models.GraphQl;

using System.Text.Json.Serialization;

public record CommentThreadNode(
    [property: JsonPropertyName("isResolved")] bool IsResolved,
    [property: JsonPropertyName("isOutdated")] bool IsOutdated,
    [property: JsonPropertyName("isCollapsed")] bool IsCollapsed,
    [property: JsonPropertyName("comments")] Comments Comments);