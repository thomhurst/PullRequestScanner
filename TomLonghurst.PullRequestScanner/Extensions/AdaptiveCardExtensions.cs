﻿using TomLonghurst.PullRequestScanner.Models.Teams;
using TomLonghurst.PullRequestScanner.Services;

namespace TomLonghurst.PullRequestScanner.Extensions;

internal static class AdaptiveCardExtensions
{
    internal static AdaptiveCardMentionedEntity[] ToAdaptiveCardMentionEntities(this IEnumerable<TeamMember> teamMembers)
    {
        return teamMembers.Distinct()
            .Where(x => !string.IsNullOrEmpty(x.Email))
            .Select(x => new AdaptiveCardMentionedEntity(
                Type: "mention",
                Text: x.ToAtMarkupTag(),
                Mentioned: new Mentioned(Id: x.Email, Name: x.DisplayOrUniqueName)
            ))
            .ToArray();
    }

    internal static string ToAtMarkupTag(this TeamMember teamMember)
    {
        if (!string.IsNullOrEmpty(teamMember.Email))
        {
            return $"<at>{teamMember.DisplayOrUniqueName}</at>";
        }

        return teamMember.DisplayOrUniqueName;
    }

    internal static void MarkCardAsWrittenTo(this MicrosoftTeamsAdaptiveCard microsoftTeamsAdaptiveCard)
    {
        microsoftTeamsAdaptiveCard.AdditionalProperties.Add("ShouldReturn", true);
    }
    
    internal static bool IsCardWrittenTo(this MicrosoftTeamsAdaptiveCard microsoftTeamsAdaptiveCard)
    {
        return microsoftTeamsAdaptiveCard.AdditionalProperties.TryGetValue("ShouldReturn", out var objBool)
               && objBool is true;
    }
}