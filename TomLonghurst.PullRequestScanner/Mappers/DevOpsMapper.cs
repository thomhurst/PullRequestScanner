using TomLonghurst.PullRequestScanner.Enums;
using TomLonghurst.PullRequestScanner.Models.DevOps;
using TomLonghurst.PullRequestScanner.Models.Self;
using TomLonghurst.PullRequestScanner.Services;
using DevOpsRepository = TomLonghurst.PullRequestScanner.Models.DevOps.Repository;
using Repository = TomLonghurst.PullRequestScanner.Models.Self.Repository;

namespace TomLonghurst.PullRequestScanner.Mappers;

internal class DevOpsMapper : IDevOpsMapper
{
    private readonly ITeamMembersService _teamMembersService;

    public DevOpsMapper(ITeamMembersService teamMembersService)
    {
        _teamMembersService = teamMembersService;
    }
    
    public PullRequest ToPullRequestModel(DevOpsPullRequestContext pullRequestContext)
    {
        var pullRequest = pullRequestContext.DevOpsPullRequest;
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
            Platform = Platform.AzureDevOps
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

    private CommentThread GetCommentThread(DevOpsPullRequestThread devOpsPullRequestThread)
    {
        return new CommentThread
        {
            Status = GetThreadStatus(devOpsPullRequestThread.ThreadStatus),
            Comments = devOpsPullRequestThread
                .Comments
                .Where(x => !x.DevOpsAuthor.UniqueName.StartsWith(Constants.VSTFSUniqueNamePrefix))
                .Where(x => x.DevOpsAuthor.DisplayName != Constants.VSTSDisplayName)
                .Select(GetComment)
                .ToList()
        };
    }

    private Comment GetComment(DevOpsComment devOpsComment)
    {
        return new Comment
        {
            LastUpdated = devOpsComment.LastUpdatedDate,
            Author = GetPerson(devOpsComment.DevOpsAuthor.UniqueName, devOpsComment.DevOpsAuthor.DisplayName)
        };
    }

    private TeamMember GetPerson(string uniqueName, string displayName)
    {
        var foundTeamMember = _teamMembersService.FindDevOpsTeamMember(uniqueName);

        if (foundTeamMember == null)
        {
            return new TeamMember
            {
                DevOpsUsername = uniqueName,
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

    private Approver GetApprover(Reviewer reviewer, IReadOnlyList<DevOpsPullRequestThread> devOpsPullRequestThreads)
    {
        return new Approver
        {
            Vote = GetVote(reviewer.Vote),
            IsRequired = reviewer.IsRequired == true,
            TeamMember = GetPerson(reviewer.UniqueName, reviewer.DisplayName),
            Time = devOpsPullRequestThreads
                .Where(x => x.Properties?.CodeReviewThreadType?.Value == "VoteUpdate")
                .LastOrDefault(x => x.Comments?.SingleOrDefault(c => c.DevOpsAuthor.UniqueName == reviewer.UniqueName) != null)
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

    private PullRequestStatus GetStatus(DevOpsPullRequestContext devOpsPullRequestContext)
    {
        if (devOpsPullRequestContext.DevOpsPullRequest.Status == "completed")
        {
            return PullRequestStatus.Completed;
        }
        
        if (devOpsPullRequestContext.DevOpsPullRequest.Status == "abandoned")
        {
            return PullRequestStatus.Abandoned;
        }
        
        if (devOpsPullRequestContext.DevOpsPullRequest.MergeStatus == "conflicts")
        {
            return PullRequestStatus.MergeConflicts;
        }

        if (devOpsPullRequestContext.DevOpsPullRequest.IsDraft)
        {
            return PullRequestStatus.Draft;
        }
        
        if (devOpsPullRequestContext.DevOpsPullRequest.Reviewers.Any(r => r.Vote == -10))
        {
            return PullRequestStatus.Rejected;
        }

        if (devOpsPullRequestContext.Iterations.Any())
        {
            var lastCommitIterationId = devOpsPullRequestContext.Iterations.Max(x => x.IterationId);
            
            var checksInLastIteration = devOpsPullRequestContext.Iterations
                .Where(x => x.IterationId == lastCommitIterationId);

            if (checksInLastIteration
                .GroupBy(x => x.Context)
                .Any(group => group.MaxBy(s => s.Id).State == "failed"))
            {
                return PullRequestStatus.FailingChecks;
            }
        }

        if (devOpsPullRequestContext.PullRequestThreads.Any(t => t.ThreadStatus is "active" or "pending"))
        {
            return PullRequestStatus.OutStandingComments;
        }
        
        if (!string.IsNullOrEmpty(devOpsPullRequestContext.DevOpsPullRequest.MergeFailureMessage))
        {
            return PullRequestStatus.FailedToMerge;
        }

        if (devOpsPullRequestContext.DevOpsPullRequest.Reviewers.Any(x => x.Vote <= 0 && x.IsRequired == true))
        {
            return PullRequestStatus.NeedsReviewing;
        }

        if (devOpsPullRequestContext.DevOpsPullRequest.Reviewers.Any(r => r.Vote > 0))
        {
            return PullRequestStatus.ReadyToMerge;
        }

        return PullRequestStatus.NeedsReviewing;
    }

    private Repository GetRepository(DevOpsRepository devOpsRepository)
    {
        return new Repository
        {
            Name = devOpsRepository.Name,
            Id = devOpsRepository.Id,
            Url = GetRepositoryUIUrl(devOpsRepository.Url)
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