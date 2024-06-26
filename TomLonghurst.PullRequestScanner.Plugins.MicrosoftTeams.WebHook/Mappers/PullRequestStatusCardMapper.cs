namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Mappers;

using System.Text;
using AdaptiveCards;
using Newtonsoft.Json;
using Enums;
using TomLonghurst.PullRequestScanner.Mappers;
using TomLonghurst.PullRequestScanner.Models;
using Extensions;
using Models;

internal class PullRequestStatusCardMapper : IPullRequestStatusCardMapper
{
    public IEnumerable<MicrosoftTeamsAdaptiveCard> Map(IReadOnlyList<PullRequest> pullRequests, PullRequestStatus pullRequestStatus)
    {
        return Map(pullRequests, pullRequestStatus, 0);
    }

    private static IEnumerable<MicrosoftTeamsAdaptiveCard> Map(IReadOnlyList<PullRequest> pullRequests, PullRequestStatus pullRequestStatus, int cardCount)
    {
        var pullRequestsWithStatus = pullRequests
            .Where(x => x.PullRequestStatus == pullRequestStatus)
            .ToList();

        if (!pullRequestsWithStatus.Any())
        {
            yield break;
        }

        var teamsNotificationCard = new MicrosoftTeamsAdaptiveCard
        {
            MsTeams = new MicrosoftTeamsProperties
            {
                Width = "full",
            },
            Body =
            [
                new AdaptiveTextBlock
                {
                    Weight = AdaptiveTextWeight.Bolder,
                    Size = AdaptiveTextSize.ExtraLarge,
                    Text = pullRequestStatus.GetMessage()
                }
            ],
        };

        var mentionedUsers = new List<TeamMember>();

        foreach (var pullRequestsInRepo in pullRequestsWithStatus.GroupBy(x => x.Repository.Id))
        {
            var adaptiveContainer = new AdaptiveContainer
            {
                Spacing = AdaptiveSpacing.ExtraLarge,
                Style = AdaptiveContainerStyle.Emphasis,
                Items =
                [
                    new AdaptiveTextBlock
                    {
                        Text =
                            $"**Repository:** [{pullRequestsInRepo.First().Repository.Name}]({pullRequestsInRepo.First().Repository.Url})"
                    },

                    new AdaptiveColumnSet
                    {
                        Columns =
                        [
                            new AdaptiveColumn
                            {
                                Items =
                                [
                                    new AdaptiveTextBlock
                                    {
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Text = "Pull Request"
                                    }
                                ]
                            },

                            new AdaptiveColumn
                            {
                                Items =
                                [
                                    new AdaptiveTextBlock
                                    {
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Text = "Author"
                                    }
                                ],
                                Width = "auto"
                            }
                        ]
                    }
                ],
            };

            teamsNotificationCard.Body.Add(adaptiveContainer);

            foreach (var pullRequest in pullRequestsInRepo)
            {
                teamsNotificationCard.MarkCardAsWrittenTo();
                pullRequestsWithStatus.Remove(pullRequest);
                mentionedUsers.Add(pullRequest.Author);
                teamsNotificationCard.MsTeams.Entitities = mentionedUsers.ToAdaptiveCardMentionEntities();

                adaptiveContainer.Items.Add(new AdaptiveColumnSet
                {
                    Columns =
                    [
                        new AdaptiveColumn
                        {
                            Items =
                            [
                                new AdaptiveTextBlock
                                {
                                    Text = $"[#{pullRequest.Number}]({pullRequest.Url}) {pullRequest.Title}"
                                }
                            ]
                        },

                        new AdaptiveColumn
                        {
                            Items =
                            [
                                new AdaptiveTextBlock
                                {
                                    Text = pullRequest.Author.ToAtMarkupTag()
                                }
                            ],
                            Width = "auto"
                        }
                    ],
                });

                var jsonString = JsonConvert.SerializeObject(teamsNotificationCard, Formatting.None);
                if (Encoding.Unicode.GetByteCount(jsonString) > Constants.TeamsCardSizeLimit)
                {
                    yield return teamsNotificationCard;

                    foreach (var microsoftTeamsAdaptiveCard in Map(pullRequestsWithStatus.ToList(), pullRequestStatus, cardCount + 1))
                    {
                        yield return microsoftTeamsAdaptiveCard;
                    }

                    yield break;
                }
            }
        }

        if (!teamsNotificationCard.IsCardWrittenTo())
        {
            yield break;
        }

        yield return teamsNotificationCard;
    }
}