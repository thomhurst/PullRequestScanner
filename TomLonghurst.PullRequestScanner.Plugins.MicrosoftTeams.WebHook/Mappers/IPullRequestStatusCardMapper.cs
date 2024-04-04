// <copyright file="IPullRequestStatusCardMapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Mappers;

using TomLonghurst.PullRequestScanner.Enums;
using TomLonghurst.PullRequestScanner.Models;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Models;

internal interface IPullRequestStatusCardMapper
{
    IEnumerable<MicrosoftTeamsAdaptiveCard> Map(IReadOnlyList<PullRequest> pullRequests, PullRequestStatus pullRequestStatus);
}