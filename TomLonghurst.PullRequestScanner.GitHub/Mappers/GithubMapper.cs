namespace TomLonghurst.PullRequestScanner.GitHub.Mappers;

using Octokit;
using Octokit.GraphQL.Model;
using Enums;
using Models;
using TomLonghurst.PullRequestScanner.Models;
using TomLonghurst.PullRequestScanner.Services;
using MergeableState = Octokit.MergeableState;
using PullRequest = TomLonghurst.PullRequestScanner.Models.PullRequest;
using Repository = TomLonghurst.PullRequestScanner.Models.Repository;

internal class GithubMapper : IGithubMapper
{
    private readonly ITeamMembersService _teamMembersService;

    public GithubMapper(ITeamMembersService teamMembersService)
    {
        _teamMembersService = teamMembersService;
    }

    public PullRequest ToPullRequestModel(GithubPullRequest githubPullRequest)
    {
        var pullRequestModel = new PullRequest
        {
            Title = githubPullRequest.Title,
            Created = githubPullRequest.Created,
            Description = githubPullRequest.Body,
            Url = githubPullRequest.Url,
            Id = githubPullRequest.Id,
            Number = githubPullRequest.PullRequestNumber.ToString(),
            Repository = GetRepository(githubPullRequest),
            IsDraft = githubPullRequest.IsDraft,
            IsActive = GetIsActive(githubPullRequest),
            PullRequestStatus = GetStatus(githubPullRequest),
            Author = GetPerson(githubPullRequest.Author),
            Approvers = githubPullRequest.Reviewers
                .Where(x => x.Author != githubPullRequest.Author)
                .Select(GetApprover)
                .ToList(),
            CommentThreads = githubPullRequest.Threads
                .Select(GetCommentThreads)
                .ToList(),
            Platform = "GitHub",
            Labels = githubPullRequest.Labels,
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

    private static bool GetIsActive(GithubPullRequest githubPullRequest)
    {
        if (githubPullRequest.IsClosed)
        {
            return false;
        }

        return githubPullRequest.State == ItemState.Open;
    }

    private TeamMember GetPerson(string author)
    {
        var foundTeamMember = _teamMembersService.FindTeamMember(author, author);

        if (foundTeamMember == null)
        {
            return new TeamMember
            {
                UniqueNames = { author },
            };
        }

        return foundTeamMember;
    }

    private CommentThread GetCommentThreads(GithubThread githubThread)
    {
        return new CommentThread
        {
            Status = GetThreadStatus(githubThread),
            Comments = githubThread.Comments.Select(GetComment).ToList(),
        };
    }

    private Comment GetComment(GithubComment githubComment)
    {
        return new Comment
        {
            LastUpdated = githubComment.LastUpdated,
            Author = GetPerson(githubComment.Author),
        };
    }

    private static ThreadStatus GetThreadStatus(GithubThread githubThread)
    {
        if (!githubThread.IsResolved)
        {
            return ThreadStatus.Active;
        }

        return ThreadStatus.Closed;
    }

    private Approver GetApprover(GithubReviewer reviewer)
    {
        return new Approver
        {
            Vote = GetVote(reviewer.State),
            IsRequired = false,
            TeamMember = GetPerson(reviewer.Author),
            Time = reviewer.LastUpdated,
        };
    }

    private static Vote GetVote(Octokit.GraphQL.Model.PullRequestReviewState vote)
    {
        if (vote == Octokit.GraphQL.Model.PullRequestReviewState.Approved)
        {
            return Vote.Approved;
        }

        return Vote.NoVote;
    }

    private static PullRequestStatus GetStatus(GithubPullRequest pullRequest)
    {
        if (pullRequest.IsMerged)
        {
            return PullRequestStatus.Completed;
        }

        if (pullRequest.State == ItemState.Closed)
        {
            return PullRequestStatus.Abandoned;
        }

        if (pullRequest.Mergeable == MergeableState.Dirty)
        {
            return PullRequestStatus.MergeConflicts;
        }

        if (pullRequest.IsDraft)
        {
            return PullRequestStatus.Draft;
        }

        if (pullRequest.ChecksStatus is StatusState.Error or StatusState.Failure)
        {
            return PullRequestStatus.FailingChecks;
        }

        if (pullRequest.Threads.Any(t => !t.IsResolved))
        {
            return PullRequestStatus.OutStandingComments;
        }

        if (pullRequest.Reviewers.Any(r => r.State == Octokit.GraphQL.Model.PullRequestReviewState.Approved))
        {
            return PullRequestStatus.ReadyToMerge;
        }

        return PullRequestStatus.NeedsReviewing;
    }

    private static Repository GetRepository(GithubPullRequest pullRequest)
    {
        return new Repository
        {
            Name = pullRequest.RepositoryName,
            Id = pullRequest.RepositoryId,
            Url = pullRequest.RepositoryUrl,
        };
    }
}