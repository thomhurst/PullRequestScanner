using System.Collections.Concurrent;
using AdaptiveCards;
using TomLonghurst.PullRequestScanner.Enums;
using TomLonghurst.PullRequestScanner.Extensions;
using TomLonghurst.PullRequestScanner.Http;
using TomLonghurst.PullRequestScanner.Mappers;
using TomLonghurst.PullRequestScanner.Mappers.TeamsCards;
using TomLonghurst.PullRequestScanner.Models.Self;
using TomLonghurst.PullRequestScanner.Models.Teams;
using TomLonghurst.PullRequestScanner.Options;

namespace TomLonghurst.PullRequestScanner.Services;

internal class PullRequestScannerNotifier : IPullRequestScannerNotifier
{
    public PullRequestScannerNotifier(MicrosoftTeamsWebhookClient microsoftTeamsWebhookClient,
        IPullRequestService pullRequestService,
        IPullRequestStatusesMapper pullRequestStatusesMapper)
    {
        _microsoftTeamsWebhookClient = microsoftTeamsWebhookClient;
        _pullRequestService = pullRequestService;
        _pullRequestStatusesMapper = pullRequestStatusesMapper;
    }

    private readonly IPullRequestService _pullRequestService;
    private readonly IPullRequestStatusesMapper _pullRequestStatusesMapper;
    private readonly MicrosoftTeamsWebhookClient _microsoftTeamsWebhookClient;

    public async Task NotifyTeamsChannel(MicrosoftTeamsPublishOptions microsoftTeamsPublishOptions)
    {
        var pullRequests = await _pullRequestService.GetPullRequests();
        await NotifyTeamsChannel(pullRequests, microsoftTeamsPublishOptions);
    }

    public async Task NotifyTeamsChannel(IReadOnlyList<PullRequest> pullRequests, MicrosoftTeamsPublishOptions microsoftTeamsPublishOptions)
    {
        if (microsoftTeamsPublishOptions.PublishPullRequestStatusesCard)
        {
            await PublishPullRequestStatuses(pullRequests);
        }
        
        if (microsoftTeamsPublishOptions.PublishPullRequestMergeConflictsCard)
        {
            await PublishStatusCard(pullRequests, PullRequestStatus.MergeConflicts);
        }
        
        if (microsoftTeamsPublishOptions.PublishPullRequestReadyToMergeCard)
        {
            await PublishStatusCard(pullRequests, PullRequestStatus.ReadyToMerge);
        }
        
        if (microsoftTeamsPublishOptions.PublishPullRequestFailingChecksCard)
        {
            await PublishStatusCard(pullRequests, PullRequestStatus.FailingChecks);
        }
        
        if (microsoftTeamsPublishOptions.PublishPullRequestReviewerLeaderboardCard)
        {
            await PublishReviewerLeaderboard(pullRequests);
        }
    }

    private async Task PublishPullRequestStatuses(IReadOnlyList<PullRequest> pullRequests)
    {
        var cards = _pullRequestStatusesMapper.Map(pullRequests);
        
        foreach (var microsoftTeamsAdaptiveCard in cards)
        {
            await _microsoftTeamsWebhookClient.CreateTeamsNotification(microsoftTeamsAdaptiveCard);
        }
    }

    private async Task PublishReviewerLeaderboard(IReadOnlyList<PullRequest> pullRequests)
    {
        if(!pullRequests.Any(x => x.Approvers.Any(a => a.Time.IsYesterday()))
           && !pullRequests.Any(x => x.AllComments.Any(c => c.LastUpdated.IsYesterday())))
        {
            return;
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
            }
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
                     .Where(x => x.Value.CommentsCount != 0 && x.Value.ReviewedCount != 0)
                     .OrderByDescending(x => x.Value.CommentsCount)
                     .ThenByDescending(x => x.Value.ReviewedCount))
        {
            
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
                                        new Uri(personsCommentsAndReview.Key.GithubImageUrl
                                        // ?? personsCommentsAndReview.Key.DevOpsImageUrl - Need to be auth'd to even do a GET on this
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
                                    Text = personsCommentsAndReview.Value.ReviewedCount.ToString()                                }
                            }
                        }
                    }
                });
        }
        
        teamsNotificationCard.MsTeams.Entitities = personsCommentsAndReviews
            .Where(x => x.Value.CommentsCount != 0 && x.Value.ReviewedCount != 0)
            .Select(x => x.Key)
            .ToAdaptiveCardMentionEntities();
        
        await _microsoftTeamsWebhookClient.CreateTeamsNotification(teamsNotificationCard);
    }

    private async Task PublishStatusCard(IReadOnlyList<PullRequest> pullRequests, PullRequestStatus pullRequestStatus)
    {
        var pullRequestsWithStatus = pullRequests
                .Where(x => x.PullRequestStatus == pullRequestStatus)
                .ToList();

        if (!pullRequestsWithStatus.Any())
        {
            return;
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

            foreach (var pullRequest in pullRequestsInRepo)
            {
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
            }

            teamsNotificationCard.Body.Add(adaptiveContainer);
        }

        teamsNotificationCard.MsTeams.Entitities = mentionedUsers.ToAdaptiveCardMentionEntities();
        
        await _microsoftTeamsWebhookClient.CreateTeamsNotification(teamsNotificationCard);
    }
}