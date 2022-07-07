using TomLonghurst.PullRequestScanner.Models.Teams;
using TomLonghurst.PullRequestScanner.Services;

namespace TomLonghurst.PullRequestScanner.Extensions;

internal static class AdaptiveCardExtensions
{
    internal static AdaptiveCardMentionedEntity[] ToAdaptiveCardMentionEntities(this IEnumerable<TeamMember> teamMembers)
    {
        return teamMembers.Distinct()
            .Where(x => x.Email == "tom.longhurst@asos.com" || x.DisplayName == "Tom Longhurst")
            .Select(x => new AdaptiveCardMentionedEntity(
                Type: "mention",
                Text: $"<at>{x.DisplayOrUniqueName}</at>",
                Mentioned: new Mentioned(Id: x.Email, Name: x.DisplayOrUniqueName)
            ))
            .ToArray();
    }
}