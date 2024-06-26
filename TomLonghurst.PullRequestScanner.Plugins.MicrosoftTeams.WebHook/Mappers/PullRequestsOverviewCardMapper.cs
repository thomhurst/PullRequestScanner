namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Mappers;

using System.Text;
using AdaptiveCards;
using Newtonsoft.Json;
using Enums;
using TomLonghurst.PullRequestScanner.Extensions;
using TomLonghurst.PullRequestScanner.Mappers;
using TomLonghurst.PullRequestScanner.Models;
using Extensions;
using Models;

internal class PullRequestsOverviewCardMapper : IPullRequestsOverviewCardMapper
{
    public IEnumerable<MicrosoftTeamsAdaptiveCard> Map(IReadOnlyList<PullRequest> pullRequests)
    {
        return Map(pullRequests, 1);
    }

    private static IEnumerable<MicrosoftTeamsAdaptiveCard> Map(IReadOnlyList<PullRequest> pullRequests, int cardCount)
    {
        var repos = pullRequests
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.Created)
            .GroupBy(x => x.Repository.Id)
            .ToMutableGrouping();

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
                    Size = AdaptiveTextSize.Large,
                    Text = $"Pull Request Statuses {GetCardNumberString(cardCount)}"
                }

            ],
        };

        var mentionedUsers = new List<TeamMember>();

        foreach (var repo in repos.ToList())
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
                            $"**Repository:** [{repo.Values.First().Repository.Name}]({repo.Values.First().Repository.Url})"
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
                                Width = "150px"
                            },

                            new AdaptiveColumn
                            {
                                Items =
                                [
                                    new AdaptiveTextBlock
                                    {
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Text = "Status"
                                    }
                                ],
                                Width = "120px"
                            },

                            new AdaptiveColumn
                            {
                                Items =
                                [
                                    new AdaptiveTextBlock
                                    {
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Text = "Age"
                                    }
                                ],
                                Width = "50px"
                            }
                        ]
                    }
                ],
            };

            teamsNotificationCard.Body.Add(adaptiveContainer);

            foreach (var pullRequest in repo.Values.ToList())
            {
                teamsNotificationCard.MarkCardAsWrittenTo();
                repo.Values.Remove(pullRequest);
                mentionedUsers.Add(pullRequest.Author);
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
                                    Text = $"[#{pullRequest.Number}]({pullRequest.Url}) {pullRequest.Title}",
                                    Color = pullRequest.IsDraft ? AdaptiveTextColor.Accent : AdaptiveTextColor.Default
                                }
                            ]
                        },

                        new AdaptiveColumn
                        {
                            Items =
                            [
                                new AdaptiveTextBlock
                                {
                                    Text = pullRequest.Author.ToAtMarkupTag(),
                                    Color = pullRequest.IsDraft ? AdaptiveTextColor.Accent : AdaptiveTextColor.Default
                                }
                            ],
                            Width = "150px"
                        },

                        new AdaptiveColumn
                        {
                            Items =
                            [
                                new AdaptiveTextBlock
                                {
                                    Text = pullRequest.PullRequestStatus.GetMessage(),
                                    Color = GetColorForStatus(pullRequest.PullRequestStatus)
                                }
                            ],
                            Width = "120px"
                        },

                        new AdaptiveColumn
                        {
                            Items =
                            [
                                new AdaptiveTextBlock
                                {
                                    Text = GetAge(pullRequest.Created),
                                    Color = GetColorForAge(pullRequest.Created)
                                }
                            ],
                            Width = "50px"
                        }
                    ],
                });

                teamsNotificationCard.MsTeams.Entitities = mentionedUsers.ToAdaptiveCardMentionEntities();

                var jsonString = JsonConvert.SerializeObject(teamsNotificationCard, Formatting.None);
                if (Encoding.Unicode.GetByteCount(jsonString) > Constants.TeamsCardSizeLimit)
                {
                    yield return teamsNotificationCard;

                    foreach (var microsoftTeamsAdaptiveCard in Map(repos.SelectMany(x => x.Values).ToList(), cardCount + 1))
                    {
                        yield return microsoftTeamsAdaptiveCard;
                    }

                    yield break;
                }
            }

            repos.Remove(repo);
        }

        if (!teamsNotificationCard.IsCardWrittenTo())
        {
            yield break;
        }

        yield return teamsNotificationCard;
    }

    private static string GetAge(DateTimeOffset dateTime)
    {
        var timeSpanSinceCreation = DateTime.UtcNow - dateTime;

        if (timeSpanSinceCreation.TotalDays >= 1)
        {
            return $"{(int)timeSpanSinceCreation.TotalDays}d";
        }

        if (timeSpanSinceCreation.TotalHours >= 1)
        {
            return $"{(int)timeSpanSinceCreation.TotalHours}h";
        }

        return $"{(int)timeSpanSinceCreation.TotalMinutes}m";
    }

    private static AdaptiveTextColor GetColorForAge(DateTimeOffset dateTime)
    {
        var timeSpanSinceCreation = DateTime.UtcNow - dateTime;

        if (timeSpanSinceCreation.TotalDays is >= 3 and < 7)
        {
            return AdaptiveTextColor.Warning;
        }

        if (timeSpanSinceCreation.TotalDays >= 7)
        {
            return AdaptiveTextColor.Attention;
        }

        return AdaptiveTextColor.Default;
    }

    private static string GetCardNumberString(int cardNumber)
    {
        return cardNumber == 1 ? string.Empty : $"Part {cardNumber}";
    }

    private static AdaptiveTextColor GetColorForStatus(PullRequestStatus status)
    {
        return status switch
        {
            PullRequestStatus.FailingChecks => AdaptiveTextColor.Attention,
            PullRequestStatus.OutStandingComments => AdaptiveTextColor.Warning,
            PullRequestStatus.NeedsReviewing => AdaptiveTextColor.Warning,
            PullRequestStatus.MergeConflicts => AdaptiveTextColor.Attention,
            PullRequestStatus.Rejected => AdaptiveTextColor.Attention,
            PullRequestStatus.ReadyToMerge => AdaptiveTextColor.Good,
            PullRequestStatus.Completed => AdaptiveTextColor.Good,
            PullRequestStatus.FailedToMerge => AdaptiveTextColor.Attention,
            PullRequestStatus.Draft => AdaptiveTextColor.Accent,
            _ => AdaptiveTextColor.Default,
        };
    }
}