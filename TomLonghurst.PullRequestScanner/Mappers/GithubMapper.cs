using Octokit.GraphQL.Model;
using TomLonghurst.PullRequestScanner.Enums;
using TomLonghurst.PullRequestScanner.Models.Github;
using TomLonghurst.PullRequestScanner.Models.Self;
using TomLonghurst.PullRequestScanner.Services.Github;
using PullRequest = TomLonghurst.PullRequestScanner.Models.Self.PullRequest;
using Repository = TomLonghurst.PullRequestScanner.Models.Self.Repository;

namespace TomLonghurst.PullRequestScanner.Mappers;

internal class GithubMapper : IGithubMapper
{
    private readonly IGithubUserService _githubUserService;

    public GithubMapper(IGithubUserService githubUserService)
    {
        _githubUserService = githubUserService;
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
            Approvers = githubPullRequest.Reviewers.Select(GetApprover).ToList(),
            CommentThreads = githubPullRequest.Threads.Select(GetCommentThreads).ToList(),
            Platform = Platform.Github
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
        
        return githubPullRequest.State == PullRequestState.Open;
    }

    private Person GetPerson(string author)
    {
        var team = _githubUserService.GetTeam();

        var foundTeamMember = team.Members.FirstOrDefault(x => x.UniqueName == author || x.DisplayName == author);

        if (foundTeamMember == null)
        {
            return new Person
            {
                DisplayName = author,
                UniqueName = author
            };
        }
        
        return new Person
        {
            DisplayName = foundTeamMember.DisplayName,
            UniqueName = foundTeamMember.UniqueName
        }; 
    }

    private CommentThread GetCommentThreads(GithubThread githubThread)
    {
        return new CommentThread
        {
            
            Status = GetThreadStatus(githubThread),
            Comments = githubThread.Comments.Select(GetComment).ToList()
        };
    }
    
    private Comment GetComment(GithubComment githubComment)
    {
        return new Comment
        {
            LastUpdated = githubComment.LastUpdated,
            Author = GetPerson(githubComment.Author)
        };
    }
    
    private ThreadStatus GetThreadStatus(GithubThread githubThread)
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
            Person = GetPerson(reviewer.Author),
            Time = reviewer.LastUpdated
        };
    }
    
    private Vote GetVote(PullRequestReviewState vote)
    {
        if (vote == PullRequestReviewState.Approved)
        {
            return Vote.Approved;
        }
    
        return Vote.NoVote;
    }
    
    private PullRequestStatus GetStatus(GithubPullRequest pullRequest)
    {
        if (pullRequest.Mergeable == MergeableState.Conflicting)
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

        if (pullRequest.State == PullRequestState.Merged)
        {
            return PullRequestStatus.Completed;
        }

        if(pullRequest.Reviewers.Any(r => r.State == PullRequestReviewState.Approved))
        {
            return PullRequestStatus.ReadyToMerge;
        }
    
        return PullRequestStatus.NeedsReviewing;
    }
    
    private Repository GetRepository(GithubPullRequest pullRequest)
    {
        return new Repository
        {
            Name = pullRequest.RepositoryName,
            Id = pullRequest.RepositoryId,
            Url = pullRequest.RepositoryUrl
        };
    }
}