using System.Collections.Concurrent;
using AdaptiveCards;
using Newtonsoft.Json;
using TomLonghurst.PullRequestScanner.Enums;
using TomLonghurst.PullRequestScanner.Extensions;
using TomLonghurst.PullRequestScanner.Http;
using TomLonghurst.PullRequestScanner.Mappers;
using TomLonghurst.PullRequestScanner.Models.Self;
using TomLonghurst.PullRequestScanner.Models.Teams;
using TomLonghurst.PullRequestScanner.Options;
using TomLonghurst.PullRequestScanner.Services.Github;

namespace TomLonghurst.PullRequestScanner.Services;

internal class PullRequestScannerNotifier : IPullRequestScannerNotifier
{
    public PullRequestScannerNotifier(MicrosoftTeamsWebhookClient microsoftTeamsWebhookClient,
        IGithubUserService githubUserService,
        IPullRequestService pullRequestService)
    {
        _microsoftTeamsWebhookClient = microsoftTeamsWebhookClient;
        _githubUserService = githubUserService;
        _pullRequestService = pullRequestService;
    }

    private readonly MicrosoftTeamsWebhookClient _microsoftTeamsWebhookClient;

    private readonly IGithubUserService _githubUserService;
    private readonly IPullRequestService _pullRequestService;

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
        var repos = pullRequests
            .Where(x => x.IsActive)
            .OrderBy(x => x.Created)
            .GroupBy(x => x.Repository.Id)
            .ToList();

        var reposIterated = 0;
        var cardsWritten = 0;

        StartOfWriteTeamsCard:
        cardsWritten++;
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
                    Text = $"Pull Request Statuses {GetCardNumberString(cardsWritten)}"
                },
            }
        };
        
        var mentionedUsers = new List<TeamMember>();

        foreach (var repo in repos.Skip(reposIterated))
        {
            if (JsonConvert.SerializeObject(teamsNotificationCard).Length > 24000)
            {
                teamsNotificationCard.MsTeams.Entitities = mentionedUsers.ToAdaptiveCardMentionEntities();
                await _microsoftTeamsWebhookClient.CreateTeamsNotification(teamsNotificationCard);
                goto StartOfWriteTeamsCard;
            }

            reposIterated++;

            var adaptiveContainer = new AdaptiveContainer
            {
                Spacing = AdaptiveSpacing.ExtraLarge,
                Style = AdaptiveContainerStyle.Emphasis,
                Items = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = $"**Repository:** [{repo.First().Repository.Name}]({repo.First().Repository.Url})"
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
            
            foreach (var pullRequest in repo.OrderByDescending(x => x.Created))
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
            }
            
            teamsNotificationCard.Body.Add(adaptiveContainer);
        }

        teamsNotificationCard.MsTeams.Entitities = mentionedUsers.ToAdaptiveCardMentionEntities();

        await _microsoftTeamsWebhookClient.CreateTeamsNotification(teamsNotificationCard);
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