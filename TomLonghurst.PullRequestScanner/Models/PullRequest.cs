namespace TomLonghurst.PullRequestScanner.Models;

using TomLonghurst.PullRequestScanner.Enums;

public record PullRequest
{
    public string Title { get; set; }

    public string Description { get; set; }

    public string Url { get; set; }

    public string Id { get; set; }

    public string Number { get; set; }

    public Repository Repository { get; set; }

    public TeamMember Author { get; set; }

    public List<CommentThread> CommentThreads { get; set; } = new();

    public List<Comment> AllComments => this.CommentThreads.SelectMany(t => t.Comments).ToList();

    public DateTimeOffset Created { get; set; }

    public bool IsActive { get; set; }

    public PullRequestStatus PullRequestStatus { get; set; }

    public bool IsDraft { get; set; }

    public List<Approver> Approvers { get; set; } = new();

    public string Platform { get; set; }

    public List<string> Labels { get; set; } = new();

    public Vote Vote
    {
        get
        {
            if (this.Approvers?.Any(x => x.Vote == Vote.Rejected) == true)
            {
                return Vote.Rejected;
            }

            if (this.Approvers?.Any(x => x.Vote != Vote.Approved && x.IsRequired) == true)
            {
                return Vote.NoVote;
            }

            if (this.Approvers?.Any(x => x.Vote == Vote.Approved) == true)
            {
                return Vote.Approved;
            }

            return Vote.NoVote;
        }
    }

    public List<TeamMember> UniqueReviewers
    {
        get
        {
            return this.Approvers
                .Where(x => x.Vote != Vote.NoVote)
                .Select(a => a.TeamMember)
                .Concat(this.AllComments.Select(c => c.Author))
                .Where(reviewer => reviewer.DisplayName != Constants.VstsDisplayName)
                .Where(reviewer => reviewer.UniqueNames.All(un => un.StartsWith(Constants.VstfsUniqueNamePrefix) != true))
                .Where(reviewer => reviewer != this.Author)
                .Distinct()
                .ToList();
        }
    }

    public int GetCommentCountWhere(TeamMember teamMember, Func<Comment, bool> condition)
    {
        if (teamMember == this.Author)
        {
            // We don't count comments on your own PR!
            return 0;
        }

        return this.CommentThreads
            .SelectMany(c => c.Comments)
            .Where(c => condition?.Invoke(c) ?? true)
            .Count(c => c.Author == teamMember);
    }

    public bool HasVotedWhere(TeamMember teamMember, Func<Approver, bool> condition)
    {
        if (teamMember == this.Author)
        {
            // You can't vote for your own PR
            return false;
        }

        return this.Approvers
            .Where(a => a.TeamMember == teamMember)
            .Where(a => condition?.Invoke(a) ?? true)
            .Any(x => x.Vote != Vote.NoVote);
    }
}