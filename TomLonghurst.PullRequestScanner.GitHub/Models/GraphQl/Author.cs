// <copyright file="Author.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.GitHub.Models.GraphQl;

using System.Text.Json.Serialization;

public record Author(
    [property: JsonPropertyName("login")] string Login);