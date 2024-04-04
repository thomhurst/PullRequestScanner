// <copyright file="AdaptiveCardMentionedEntity.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Models;

using System.Text.Json.Serialization;

internal record AdaptiveCardMentionedEntity(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("mentioned")]
    Mentioned Mentioned);