// <copyright file="ReviewThreads.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.GitHub.Models.GraphQl;

using System.Text.Json.Serialization;

public record ReviewThreads(
    [property: JsonPropertyName("edges")] IReadOnlyList<Edge> Edges);