// <copyright file="MicrosoftTeamsOptions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Options;

public class MicrosoftTeamsOptions
{
    /**
     * <summary>Gets or sets the webhook URL to send Teams Cards to.</summary>
     */
    public Uri? WebHookUri { get; set; }

    public MicrosoftTeamsPublishOptions PublishOptions { get; set; } = new();
}