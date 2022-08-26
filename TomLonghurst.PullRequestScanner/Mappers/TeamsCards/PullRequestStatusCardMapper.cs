using System.Text;
using AdaptiveCards;
using Newtonsoft.Json;
using TomLonghurst.PullRequestScanner.Enums;
using TomLonghurst.PullRequestScanner.Extensions;
using TomLonghurst.PullRequestScanner.Models.Self;
using TomLonghurst.PullRequestScanner.Models.Teams;
using TomLonghurst.PullRequestScanner.Services;

namespace TomLonghurst.PullRequestScanner.Mappers.TeamsCards;

internal class PullRequestStatusCardMapper : IPullRequestStatusCardMapper
{
    public IEnumerable<MicrosoftTeamsAdaptiveCard> Map(IReadOnlyList<PullRequest> pullRequests, PullRequestStatus pullRequestStatus)
    {
        return Map(pullRequests, pullRequestStatus, 0);
    }

    private IEnumerable<MicrosoftTeamsAdaptiveCard> Map(IReadOnlyList<PullRequest> pullRequests, PullRequestStatus pullRequestStatus, int cardCound)
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
                Width = "full"
            },
            Body = new List<AdaptiveElement>
            {
                new AdaptiveTextBlock
                {
                    Weight = AdaptiveTextWeight.Bolder,
                    Size = AdaptiveTextSize.ExtraLarge,
                    Text = pullRequestStatus.GetMessage()
                }
            }
        };

        var mentionedUsers = new List<TeamMember>();

        foreach (var pullRequestsInRepo in pullRequestsWithStatus.GroupBy(x => x.Repository.Id))
        {
            var adaptiveContainer = new AdaptiveContainer
            {
                Spacing = AdaptiveSpacing.ExtraLarge,
                Style = AdaptiveContainerStyle.Emphasis,
                Items = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text =
                            $"**Repository:** [{pullRequestsInRepo.First().Repository.Name}]({pullRequestsInRepo.First().Repository.Url})"
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
                                Width = "auto"
                            }
                        }
                    }
                }
            };
            
            teamsNotificationCard.Body.Add(adaptiveContainer);

            foreach (var pullRequest in pullRequestsInRepo)
            {
                teamsNotificationCard.AdditionalProperties.Add("ShouldReturn", true);
                pullRequestsWithStatus.Remove(pullRequest);
                mentionedUsers.Add(pullRequest.Author);
                teamsNotificationCard.MsTeams.Entitities = mentionedUsers.ToAdaptiveCardMentionEntities();

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
                                    Text = $"[#{pullRequest.Number}]({pullRequest.Url}) {pullRequest.Title}"
                                }
                            }
                        },
                        new()
                        {
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = pullRequest.Author.ToAtMarkupTag()
                                }
                            },
                            Width = "auto"
                        }
                    }
                });
                
                var jsonString = JsonConvert.SerializeObject(teamsNotificationCard, Formatting.None);
                if (Encoding.Unicode.GetByteCount(jsonString) > Constants.TeamsCardSizeLimit)
                {
                    yield return teamsNotificationCard;
                
                    foreach (var microsoftTeamsAdaptiveCard in Map(pullRequestsWithStatus.ToList(), pullRequestStatus, cardCound + 1))
                    {
                        yield return microsoftTeamsAdaptiveCard;
                    }
                
                    yield break;
                }
            }
        }
        
        if (!teamsNotificationCard.AdditionalProperties.TryGetValue("ShouldReturn", out _))
        {
            yield break;
        }

        yield return teamsNotificationCard;
    }
}