// <copyright file="IPullRequestLeaderboardCardMapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Mappers;

using TomLonghurst.PullRequestScanner.Models;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Models;

internal interface IPullRequestLeaderboardCardMapper
{
    IEnumerable<MicrosoftTeamsAdaptiveCard> Map(IReadOnlyList<PullRequest> pullRequests);
}