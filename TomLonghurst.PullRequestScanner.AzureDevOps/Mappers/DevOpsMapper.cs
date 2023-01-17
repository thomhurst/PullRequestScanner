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
                .Where(x => !x.UniqueName.StartsWith(Constants.VSTFSUniqueNamePrefix))
                .Where(x => x.DisplayName != Constants.VSTSDisplayName)
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

    private CommentThread GetCommentThread(AzureDevOpsPullRequestThread AzureDevOpsPullRequestThread)
    {
        return new CommentThread
        {
            Status = GetThreadStatus(AzureDevOpsPullRequestThread.ThreadStatus),
            Comments = AzureDevOpsPullRequestThread
                .Comments
                .Where(x => !x.AzureDevOpsAuthor.UniqueName.StartsWith(Constants.VSTFSUniqueNamePrefix))
                .Where(x => x.AzureDevOpsAuthor.DisplayName != Constants.VSTSDisplayName)
                .Select(GetComment)
                .ToList()
        };
    }

    private Comment GetComment(AzureDevOpsComment AzureDevOpsComment)
    {
        return new Comment
        {
            LastUpdated = AzureDevOpsComment.LastUpdatedDate,
            Author = GetPerson(AzureDevOpsComment.AzureDevOpsAuthor.UniqueName, AzureDevOpsComment.AzureDevOpsAuthor.DisplayName)
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

    private Approver GetApprover(Reviewer reviewer, IReadOnlyList<AzureDevOpsPullRequestThread> AzureDevOpsPullRequestThreads)
    {
        return new Approver
        {
            Vote = GetVote(reviewer.Vote),
            IsRequired = reviewer.IsRequired == true,
            TeamMember = GetPerson(reviewer.UniqueName, reviewer.DisplayName),
            Time = AzureDevOpsPullRequestThreads
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

    private PullRequestStatus GetStatus(AzureDevOpsPullRequestContext AzureDevOpsPullRequestContext)
    {
        if (AzureDevOpsPullRequestContext.AzureDevOpsPullRequest.Status == "completed")
        {
            return PullRequestStatus.Completed;
        }
        
        if (AzureDevOpsPullRequestContext.AzureDevOpsPullRequest.Status == "abandoned")
        {
            return PullRequestStatus.Abandoned;
        }
        
        if (AzureDevOpsPullRequestContext.AzureDevOpsPullRequest.MergeStatus == "conflicts")
        {
            return PullRequestStatus.MergeConflicts;
        }

        if (AzureDevOpsPullRequestContext.AzureDevOpsPullRequest.IsDraft)
        {
            return PullRequestStatus.Draft;
        }
        
        if (AzureDevOpsPullRequestContext.AzureDevOpsPullRequest.Reviewers.Any(r => r.Vote == -10))
        {
            return PullRequestStatus.Rejected;
        }

        if (AzureDevOpsPullRequestContext.Iterations.Any())
        {
            var lastCommitIterationId = AzureDevOpsPullRequestContext.Iterations.Max(x => x.IterationId);
            
            var checksInLastIteration = AzureDevOpsPullRequestContext.Iterations
                .Where(x => x.IterationId == lastCommitIterationId);

            if (checksInLastIteration
                .GroupBy(x => x.Context)
                .Any(group => group.MaxBy(s => s.Id).State == "failed"))
            {
                return PullRequestStatus.FailingChecks;
            }
        }

        if (AzureDevOpsPullRequestContext.PullRequestThreads.Any(t => t.ThreadStatus is "active" or "pending"))
        {
            return PullRequestStatus.OutStandingComments;
        }
        
        if (!string.IsNullOrEmpty(AzureDevOpsPullRequestContext.AzureDevOpsPullRequest.MergeFailureMessage))
        {
            return PullRequestStatus.FailedToMerge;
        }

        if (AzureDevOpsPullRequestContext.AzureDevOpsPullRequest.Reviewers.Any(x => x.Vote <= 0 && x.IsRequired == true))
        {
            return PullRequestStatus.NeedsReviewing;
        }

        if (AzureDevOpsPullRequestContext.AzureDevOpsPullRequest.Reviewers.Any(r => r.Vote > 0))
        {
            return PullRequestStatus.ReadyToMerge;
        }

        return PullRequestStatus.NeedsReviewing;
    }

    private Repository GetRepository(AzureDevOpsRepository AzureDevOpsRepository)
    {
        return new Repository
        {
            Name = AzureDevOpsRepository.Name,
            Id = AzureDevOpsRepository.Id,
            Url = GetRepositoryUIUrl(AzureDevOpsRepository.Url)
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
    
    private string GetRepositoryUIUrl(string repositoryUrl)
    {
        return repositoryUrl
            .Replace("/git/", "/_git/")
            .Replace("/repositories/", "/")
            .Replace("/_apis/", "/");
    }
}