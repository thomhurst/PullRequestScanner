namespace TomLonghurst.PullRequestScanner.AzureDevOps.Mappers;

using Microsoft.TeamFoundation.SourceControl.WebApi;
using Models;
using TomLonghurst.PullRequestScanner.Models;
using TomLonghurst.PullRequestScanner.Services;
using Comment = TomLonghurst.PullRequestScanner.Models.Comment;
using CommentThread = TomLonghurst.PullRequestScanner.Models.CommentThread;
using PullRequestStatus = Enums.PullRequestStatus;
using Repository = TomLonghurst.PullRequestScanner.Models.Repository;
using TeamFoundation = Microsoft.TeamFoundation;

internal class AzureDevOpsMapper(ITeamMembersService teamMembersService) : IAzureDevOpsMapper
{
    public PullRequest ToPullRequestModel(AzureDevOpsPullRequestContext pullRequestContext)
    {
        var pullRequest = pullRequestContext.AzureDevOpsPullRequest;
        var pullRequestModel = new PullRequest
        {
            Title = pullRequest.Title,
            Created = pullRequest.CreationDate,
            Description = pullRequest.Description,
            Url = GetPullRequestUiUrl(pullRequest.Url),
            Id = pullRequest.PullRequestId.ToString(),
            Number = pullRequest.PullRequestId.ToString(),
            Repository = GetRepository(pullRequest.Repository),
            IsDraft = pullRequest.IsDraft ?? false,
            IsActive = pullRequest.Status == TeamFoundation.SourceControl.WebApi.PullRequestStatus.Active,
            PullRequestStatus = GetStatus(pullRequestContext),
            Author = GetPerson(pullRequest.CreatedBy.UniqueName, pullRequest.CreatedBy.DisplayName, pullRequest.CreatedBy.Id),
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
            Platform = "AzureDevOps",
            Labels = pullRequest.Labels?.Where(x => x.Active != false).Select(x => x.Name).ToList() ?? [],
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

    private CommentThread GetCommentThread(GitPullRequestCommentThread azureDevOpsPullRequestThread)
    {
        return new CommentThread
        {
            Status = GetThreadStatus(azureDevOpsPullRequestThread.Status),
            Comments = azureDevOpsPullRequestThread
                .Comments
                .Where(x => !x.Author.UniqueName.StartsWith(Constants.VstfsUniqueNamePrefix))
                .Where(x => x.Author.DisplayName != Constants.VstsDisplayName)
                .Select(GetComment)
                .ToList(),
        };
    }

    private Comment GetComment(TeamFoundation.SourceControl.WebApi.Comment azureDevOpsComment)
    {
        return new Comment
        {
            LastUpdated = azureDevOpsComment.LastUpdatedDate,
            Author = GetPerson(azureDevOpsComment.Author.UniqueName, azureDevOpsComment.Author.DisplayName, azureDevOpsComment.Author.Id),
        };
    }

    private TeamMember GetPerson(string uniqueName, string displayName, string id)
    {
        var foundTeamMember = teamMembersService.FindTeamMember(uniqueName, id);

        if (foundTeamMember == null)
        {
            return new TeamMember
            {
                UniqueNames = { uniqueName },
                DisplayName = displayName,
            };
        }

        return foundTeamMember;
    }

    private static ThreadStatus GetThreadStatus(CommentThreadStatus threadStatus)
    {
        if (threadStatus is CommentThreadStatus.Active or CommentThreadStatus.Pending)
        {
            return ThreadStatus.Active;
        }

        return ThreadStatus.Closed;
    }

    private Approver GetApprover(IdentityRefWithVote reviewer, List<GitPullRequestCommentThread> azureDevOpsPullRequestThreads)
    {
        return new Approver
        {
            Vote = GetVote(reviewer.Vote),
            IsRequired = reviewer.IsRequired,
            TeamMember = GetPerson(reviewer.UniqueName, reviewer.DisplayName, reviewer.Id),
            Time = azureDevOpsPullRequestThreads
                .Where(x => x.Properties.GetValue("codeReviewThreadType", string.Empty) == "VoteUpdate")
                .LastOrDefault(x => x.Comments?.SingleOrDefault(c => c.Author.UniqueName == reviewer.UniqueName) != null)
                ?.LastUpdatedDate,
        };
    }

    private static Vote GetVote(int? vote)
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

    private static PullRequestStatus GetStatus(AzureDevOpsPullRequestContext azureDevOpsPullRequestContext)
    {
        if (azureDevOpsPullRequestContext.AzureDevOpsPullRequest.Status == TeamFoundation.SourceControl.WebApi.PullRequestStatus.Completed)
        {
            return PullRequestStatus.Completed;
        }

        if (azureDevOpsPullRequestContext.AzureDevOpsPullRequest.Status == TeamFoundation.SourceControl.WebApi.PullRequestStatus.Abandoned)
        {
            return PullRequestStatus.Abandoned;
        }

        if (azureDevOpsPullRequestContext.AzureDevOpsPullRequest.MergeStatus == PullRequestAsyncStatus.Conflicts)
        {
            return PullRequestStatus.MergeConflicts;
        }

        if (azureDevOpsPullRequestContext.AzureDevOpsPullRequest.IsDraft == true)
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
                .Any(group => group.MaxBy(s => s.Id)?.State == GitStatusState.Failed))
            {
                return PullRequestStatus.FailingChecks;
            }
        }

        if (azureDevOpsPullRequestContext.PullRequestThreads.Any(t => t.Status is CommentThreadStatus.Active or CommentThreadStatus.Pending))
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

    private static Repository GetRepository(GitRepository azureDevOpsRepository)
    {
        return new Repository
        {
            Name = azureDevOpsRepository.Name,
            Id = azureDevOpsRepository.Id.ToString(),
            Url = GetRepositoryUiUrl(azureDevOpsRepository.Url),
        };
    }

    private static string GetPullRequestUiUrl(string pullRequestUrl)
    {
        return pullRequestUrl
            .Replace("pullRequests", "pullrequest")
            .Replace("/git/", "/_git/")
            .Replace("/repositories/", "/")
            .Replace("/_apis/", "/");
    }

    private static string GetRepositoryUiUrl(string repositoryUrl)
    {
        return repositoryUrl
            .Replace("/git/", "/_git/")
            .Replace("/repositories/", "/")
            .Replace("/_apis/", "/");
    }
}