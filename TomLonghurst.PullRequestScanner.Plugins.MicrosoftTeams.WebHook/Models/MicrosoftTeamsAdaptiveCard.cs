// <copyright file="MicrosoftTeamsAdaptiveCard.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Models
{
    using AdaptiveCards;
    using Newtonsoft.Json;

    internal class MicrosoftTeamsAdaptiveCard : AdaptiveCard
    {
        public MicrosoftTeamsAdaptiveCard()
            : base(new AdaptiveSchemaVersion(1, 3))
        {
        }

        [JsonProperty("msTeams")]
        public MicrosoftTeamsProperties MsTeams { get; set; }
    }
}