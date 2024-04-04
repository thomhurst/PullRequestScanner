// <copyright file="Attachment.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Models;

using Newtonsoft.Json;

internal class Attachment
{
    [JsonProperty("contentType")]
    public string ContentType { get; set; }

    [JsonProperty("content")]
    public MicrosoftTeamsAdaptiveCard Content { get; set; }
}