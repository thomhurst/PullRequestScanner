// <copyright file="NuGetSettings.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Pipeline.Settings;

public record NuGetSettings
{
    public string? ApiKey { get; init; }
}
