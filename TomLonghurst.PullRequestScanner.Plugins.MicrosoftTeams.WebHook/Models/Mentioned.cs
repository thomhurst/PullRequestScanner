// <copyright file="Mentioned.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Models;

using System.Text.Json.Serialization;

internal record Mentioned(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name);