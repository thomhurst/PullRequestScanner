using System.Text;
using AdaptiveCards;
using Newtonsoft.Json;
using TomLonghurst.PullRequestScanner.Enums;
using TomLonghurst.PullRequestScanner.Extensions;
using TomLonghurst.PullRequestScanner.Models.Self;
using TomLonghurst.PullRequestScanner.Models.Teams;
using TomLonghurst.PullRequestScanner.Services;

namespace TomLonghurst.PullRequestScanner.Mappers.TeamsCards;

internal class PullRequestStatusesMapper : IPullRequestStatusesMapper
{
    private const int _TeamsCardSizeLimit = 22000;

    public IEnumerable<MicrosoftTeamsAdaptiveCard> Map(IReadOnlyList<PullRequest> pullRequests)
    {
        return Map(pullRequests, 1);
    }

    public IEnumerable<MicrosoftTeamsAdaptiveCard> Map(IReadOnlyList<PullRequest> pullRequests, int cardCound)
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
                Width = "full"
            },
            Body = new List<AdaptiveElement>
            {
                new AdaptiveTextBlock
                {
                    Weight = AdaptiveTextWeight.Bolder,
                    Size = AdaptiveTextSize.Large,
                    Text = $"Pull Request Statuses {GetCardNumberString(cardCound)}"
                },
            }
        };
        
        var mentionedUsers = new List<TeamMember>();

        foreach (var repo in repos.ToList())
        {
            var adaptiveContainer = new AdaptiveContainer
            {
                Spacing = AdaptiveSpacing.ExtraLarge,
                Style = AdaptiveContainerStyle.Emphasis,
                Items = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = $"**Repository:** [{repo.Values.First().Repository.Name}]({repo.Values.First().Repository.Url})"
                    },
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new()
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Text = "Pull Request"
                                    }
                                }
                            },
                            new()
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Text = "Author"
                                    }
                                },
                                Width = "150px"
                            },
                            new()
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Text = "Status"
                                    }
                                },
                                Width = "120px"
                            },
                            new()
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Text = "Age"
                                    }
                                },
                                Width = "50px"
                            }
                        }
                    }
                }
            };
            
            teamsNotificationCard.Body.Add(adaptiveContainer);

            foreach (var pullRequest in repo.Values.ToList())
            {
                repo.Values.Remove(pullRequest);
                mentionedUsers.Add(pullRequest.Author);
                adaptiveContainer.Items.Add(new AdaptiveColumnSet
                {
                    Columns = new List<AdaptiveColumn>
                    {
                        new()
                        {
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = $"[#{pullRequest.Number}]({pullRequest.Url}) {pullRequest.Title}",
                                    Color = pullRequest.IsDraft ? AdaptiveTextColor.Accent : AdaptiveTextColor.Default
                                }
                            }
                        },
                        new()
                        {
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = pullRequest.Author.ToAtMarkupTag(),
                                    Color = pullRequest.IsDraft ? AdaptiveTextColor.Accent : AdaptiveTextColor.Default
                                }
                            },
                            Width = "150px"
                        },
                        new()
                        {
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = pullRequest.PullRequestStatus.GetMessage(),
                                    Color = GetColorForStatus(pullRequest.PullRequestStatus)
                                }
                            },
                            Width = "120px"
                        },
                        new()
                        {
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = GetAge(pullRequest.Created),
                                    Color = GetColorForAge(pullRequest.Created)
                                }
                            },
                            Width = "50px"
                        }
                    }
                });
                
                teamsNotificationCard.MsTeams.Entitities = mentionedUsers.ToAdaptiveCardMentionEntities();

                var jsonString = JsonConvert.SerializeObject(teamsNotificationCard, Formatting.None);
                if (Encoding.Unicode.GetByteCount(jsonString) > _TeamsCardSizeLimit)
                {
                    yield return teamsNotificationCard;
                
                    foreach (var microsoftTeamsAdaptiveCard in Map(repos.SelectMany(x => x.Values).ToList(), cardCound + 1))
                    {
                        yield return microsoftTeamsAdaptiveCard;
                    }
                
                    yield break;
                }
            }
            
            repos.Remove(repo);
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

    private AdaptiveTextColor GetColorForAge(DateTimeOffset dateTime)
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

    private string GetCardNumberString(int cardNumber)
    {
        return cardNumber == 1 ? string.Empty : $"Part {cardNumber}";
    }

    private AdaptiveTextColor GetColorForStatus(PullRequestStatus status)
    {
        switch (status)
        {
            case PullRequestStatus.FailingChecks:
                return AdaptiveTextColor.Attention;
            case PullRequestStatus.OutStandingComments:
                return AdaptiveTextColor.Warning;
            case PullRequestStatus.NeedsReviewing:
                return AdaptiveTextColor.Warning;
            case PullRequestStatus.MergeConflicts:
                return AdaptiveTextColor.Attention;
            case PullRequestStatus.Rejected:
                return AdaptiveTextColor.Attention;
            case PullRequestStatus.ReadyToMerge:
                return AdaptiveTextColor.Good;
            case PullRequestStatus.Completed:
                return AdaptiveTextColor.Good;
            case PullRequestStatus.FailedToMerge:
                return AdaptiveTextColor.Attention;
            case PullRequestStatus.Draft:
                return AdaptiveTextColor.Accent;
            default:
                return AdaptiveTextColor.Default;
        }
    }
}