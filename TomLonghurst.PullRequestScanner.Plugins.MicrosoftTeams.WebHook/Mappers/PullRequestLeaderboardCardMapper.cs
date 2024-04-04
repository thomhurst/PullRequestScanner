// <copyright file="PullRequestLeaderboardCardMapper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Mappers;

using System.Collections.Concurrent;
using AdaptiveCards;
using TomLonghurst.PullRequestScanner.Extensions;
using TomLonghurst.PullRequestScanner.Models;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Extensions;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Models;

internal class PullRequestLeaderboardCardMapper : IPullRequestLeaderboardCardMapper
{
    public IEnumerable<MicrosoftTeamsAdaptiveCard> Map(IReadOnlyList<PullRequest> pullRequests)
    {
        return Map(pullRequests.ToList());
    }

    private static IEnumerable<MicrosoftTeamsAdaptiveCard> Map(List<PullRequest> pullRequests)
    {
        if (!pullRequests.Any(x => x.Approvers.Any(a => a.Time.IsYesterday()))
            && !pullRequests.Any(x => x.AllComments.Any(c => c.LastUpdated.IsYesterday())))
        {
            yield break;
        }

        var teamsNotificationCard = new MicrosoftTeamsAdaptiveCard
        {
            MsTeams = new MicrosoftTeamsProperties
            {
                Width = "full",
            },
            Body = new List<AdaptiveElement>
            {
                new AdaptiveTextBlock
                {
                    Weight = AdaptiveTextWeight.Bolder,
                    Size = AdaptiveTextSize.ExtraLarge,
                    Text = "Yesterday's Pull Request Reviewer Leaderboard"
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
                                    Text = "Image"
                                }
                            },
                            Width = "50px"
                        },
                        new()
                        {
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Weight = AdaptiveTextWeight.Bolder,
                                    Text = "Name"
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
                                    Text = "Comments"
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
                                    Text = "Pull Requests Reviewed"
                                }
                            }
                        }
                    }
                }
            },
        };

        var personsCommentsAndReviews = new ConcurrentDictionary<TeamMember, PullRequestReviewLeaderboardModel>();
        foreach (var pullRequest in pullRequests)
        {
            var uniqueReviewers = pullRequest.UniqueReviewers;
            uniqueReviewers.ForEach(uniqueReviewer =>
            {
                var yesterdaysCommentCount = pullRequest.GetCommentCountWhere(uniqueReviewer, c => c.LastUpdated.IsYesterday());
                var hasVoted = pullRequest.HasVotedWhere(uniqueReviewer, a => a.Vote != Vote.NoVote && a.Time.IsYesterday());

                var record = personsCommentsAndReviews.GetOrAdd(uniqueReviewer, new PullRequestReviewLeaderboardModel());

                record.CommentsCount += yesterdaysCommentCount;

                if (yesterdaysCommentCount != 0 || hasVoted)
                {
                    record.ReviewedCount++;
                }
            });
        }

        foreach (var personsCommentsAndReview in personsCommentsAndReviews
                     .Where(x => x.Value.CommentsCount != 0 || x.Value.ReviewedCount != 0)
                     .OrderByDescending(x => x.Value.CommentsCount)
                     .ThenByDescending(x => x.Value.ReviewedCount))
        {
            teamsNotificationCard.MarkCardAsWrittenTo();
            teamsNotificationCard.Body.Add(
                new AdaptiveColumnSet
                {
                    Columns = new List<AdaptiveColumn>
                    {
                        new()
                        {
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveImage
                                {
                                    PixelWidth = 50,
                                    PixelHeight = 50,
                                    Url =
                                        new Uri(
                                            personsCommentsAndReview.Key.ImageUrls.FirstOrDefault()
                                            ?? "https://github.githubassets.com/images/modules/logos_page/GitHub-Mark.png")
                                }
                            },
                            Width = "50px"
                        },
                        new()
                        {
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = personsCommentsAndReview.Key.ToAtMarkupTag()
                                }
                            }
                        },
                        new()
                        {
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = personsCommentsAndReview.Value.CommentsCount.ToString()
                                }
                            }
                        },
                        new()
                        {
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Text = personsCommentsAndReview.Value.ReviewedCount.ToString()
                                }
                            }
                        }
                    },
                });
        }

        teamsNotificationCard.MsTeams.Entitities = personsCommentsAndReviews
            .Where(x => x.Value.CommentsCount != 0 || x.Value.ReviewedCount != 0)
            .Select(x => x.Key)
            .ToAdaptiveCardMentionEntities();

        if (!teamsNotificationCard.IsCardWrittenTo())
        {
            yield break;
        }

        yield return teamsNotificationCard;
    }
}