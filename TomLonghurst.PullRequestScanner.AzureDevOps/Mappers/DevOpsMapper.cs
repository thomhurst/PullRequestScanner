using TomLonghurst.PullRequestScanner.AzureDevOps.Models;
using TomLonghurst.PullRequestScanner.Enums;
using TomLonghurst.PullRequestScanner.Models;
using TomLonghurst.PullRequestScanner.Services;
using AzureDevOpsRepository = TomLonghurst.PullRequestScanner.AzureDevOps.Models.Repository;
using Repository = TomLonghurst.PullRequestScanner.Models.Repository;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Mappers;

internal class AzureDevOpsMapper : IAzureDevOpsMapper
{
    private readonly ITeamMembersService _teamMembersService;

    public AzureDevOpsMapper(ITeamMembersService teamMembersService)
    {
        _teamMembersService = teamMembersService;
    }
    
    public PullRequest ToPullRequestModel(AzureDevOpsPullRequestContext pullRequestContext)
    {
        var pullRequest = pullRequestContext.AzureDevOpsPullRequest;
        var pullRequestModel = new PullRequest
        {
            Title = pullRequest.Title,
            Created = pullRequest.CreationDate,
            Description = pullRequest.Description,
            Url = GetPullRequestUIUrl(pullRequest.Url),
            Id = pullRequest.PullRequestId.ToString(),
            Number = pullRequest.PullRequestId.ToString(),
            Repository = GetRepository(pullRequest.Repository),
            IsDraft = pullRequest.IsDraft,
            IsActive = pullRequest.Status == "active",
            PullRequestStatus = GetStatus(pullRequestContext),
            Author = GetPerson(pullRequest.CreatedBy.UniqueName, pullRequest.CreatedBy.DisplayName),
            Approvers = pullRequest.Reviewers
                .Where(x => x.Vote != 0)
                .Where(x => x.UniqueName != pullRequest.CreatedBy.UniqueName)
                .Where(x => !x.UniqueName.StartsWith(Constants.VstfsUniqueNamePrefix))
                .Where(x => x.DisplayName != Constants.VstsDisplayName)
                .Select(r => GetApprover(r, pullRequestContext.PullRequestThreads))
                .ToList(),
            CommentThreads = pullRequestContext.PullRequestThreads
                .Select(GetCommentThread)
                .ToList(),
            Platform = "AzureDevOps"
        };
        
        foreach (var thread in pullRequestModel.CommentThreads)
        {
            thread.ParentPullRequest = pullRequestModel;
            foreach (var comment in thread.Comments)
            {
                comment.ParentCommentThread = thread;
            }
        }

        foreach (var approver in pullRequestModel.Approvers)
        {
            approver.PullRequest = pullRequestModel;
        }
        
        return pullRequestModel;
    }

    private CommentThread GetCommentThread(AzureDevOpsPullRequestThread azureDevOpsPullRequestThread)
    {
        return new CommentThread
        {
            Status = GetThreadStatus(azureDevOpsPullRequestThread.ThreadStatus),
            Comments = azureDevOpsPullRequestThread
                .Comments
                .Where(x => !x.AzureDevOpsAuthor.UniqueName.StartsWith(Constants.VstfsUniqueNamePrefix))
                .Where(x => x.AzureDevOpsAuthor.DisplayName != Constants.VstsDisplayName)
                .Select(GetComment)
                .ToList()
        };
    }

    private Comment GetComment(AzureDevOpsComment azureDevOpsComment)
    {
        return new Comment
        {
            LastUpdated = azureDevOpsComment.LastUpdatedDate,
            Author = GetPerson(azureDevOpsComment.AzureDevOpsAuthor.UniqueName, azureDevOpsComment.AzureDevOpsAuthor.DisplayName)
        };
    }

    private TeamMember GetPerson(string uniqueName, string displayName)
    {
        var foundTeamMember = _teamMembersService.FindTeamMember(uniqueName);

        if (foundTeamMember == null)
        {
            return new TeamMember
            {
                UniqueNames = { uniqueName },
                DisplayName = displayName
            };
        }

        return foundTeamMember;
    }

    private ThreadStatus GetThreadStatus(string threadStatus)
    {
        if (threadStatus is "active" or "pending")
        {
            return ThreadStatus.Active;
        }

        return ThreadStatus.Closed;
    }

    private Approver GetApprover(Reviewer reviewer, IReadOnlyList<AzureDevOpsPullRequestThread> azureDevOpsPullRequestThreads)
    {
        return new Approver
        {
            Vote = GetVote(reviewer.Vote),
            IsRequired = reviewer.IsRequired == true,
            TeamMember = GetPerson(reviewer.UniqueName, reviewer.DisplayName),
            Time = azureDevOpsPullRequestThreads
                .Where(x => x.Properties?.CodeReviewThreadType?.Value == "VoteUpdate")
                .LastOrDefault(x => x.Comments?.SingleOrDefault(c => c.AzureDevOpsAuthor.UniqueName == reviewer.UniqueName) != null)
                ?.LastUpdatedDate
        };
    }
    
    private Vote GetVote(int? vote)
    {
        if (vote is null or 0)
        {
            return Vote.NoVote;
        }

        if (vote > 0)
        {
            return Vote.Approved;
        }

        return Vote.Rejected;
    }

    private PullRequestStatus GetStatus(AzureDevOpsPullRequestContext azureDevOpsPullRequestContext)
    {
        if (azureDevOpsPullRequestContext.AzureDevOpsPullRequest.Status == "completed")
        {
            return PullRequestStatus.Completed;
        }
        
        if (azureDevOpsPullRequestContext.AzureDevOpsPullRequest.Status == "abandoned")
        {
            return PullRequestStatus.Abandoned;
        }
        
        if (azureDevOpsPullRequestContext.AzureDevOpsPullRequest.MergeStatus == "conflicts")
        {
            return PullRequestStatus.MergeConflicts;
        }

        if (azureDevOpsPullRequestContext.AzureDevOpsPullRequest.IsDraft)
        {
            return PullRequestStatus.Draft;
        }
        
        if (azureDevOpsPullRequestContext.AzureDevOpsPullRequest.Reviewers.Any(r => r.Vote == -10))
        {
            return PullRequestStatus.Rejected;
        }

        if (azureDevOpsPullRequestContext.Iterations.Any())
        {
            var lastCommitIterationId = azureDevOpsPullRequestContext.Iterations.Max(x => x.IterationId);
            
            var checksInLastIteration = azureDevOpsPullRequestContext.Iterations
                .Where(x => x.IterationId == lastCommitIterationId);

            if (checksInLastIteration
                .GroupBy(x => x.Context)
                .Any(group => group.MaxBy(s => s.Id).State == "failed"))
            {
                return PullRequestStatus.FailingChecks;
            }
        }

        if (azureDevOpsPullRequestContext.PullRequestThreads.Any(t => t.ThreadStatus is "active" or "pending"))
        {
            return PullRequestStatus.OutStandingComments;
        }
        
        if (!string.IsNullOrEmpty(azureDevOpsPullRequestContext.AzureDevOpsPullRequest.MergeFailureMessage))
        {
            return PullRequestStatus.FailedToMerge;
        }

        if (azureDevOpsPullRequestContext.AzureDevOpsPullRequest.Reviewers.Any(x => x.Vote <= 0 && x.IsRequired == true))
        {
            return PullRequestStatus.NeedsReviewing;
        }

        if (azureDevOpsPullRequestContext.AzureDevOpsPullRequest.Reviewers.Any(r => r.Vote > 0))
        {
            return PullRequestStatus.ReadyToMerge;
        }

        return PullRequestStatus.NeedsReviewing;
    }

    private Repository GetRepository(AzureDevOpsRepository azureDevOpsRepository)
    {
        return new Repository
        {
            Name = azureDevOpsRepository.Name,
            Id = azureDevOpsRepository.Id,
            Url = GetRepositoryUiUrl(azureDevOpsRepository.Url)
        };
    }
    
    private string GetPullRequestUIUrl(string pullRequestUrl)
    {
        return pullRequestUrl
            .Replace("pullRequests", "pullrequest")
            .Replace("/git/", "/_git/")
            .Replace("/repositories/", "/")
            .Replace("/_apis/", "/");
    }
    
    private string GetRepositoryUiUrl(string repositoryUrl)
    {
        return repositoryUrl
            .Replace("/git/", "/_git/")
            .Replace("/repositories/", "/")
            .Replace("/_apis/", "/");
    }
}