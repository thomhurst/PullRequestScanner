namespace TomLonghurst.PullRequestScanner.Models.Self;

public record CommentThread
{
    public PullRequest ParentPullRequest { get; set; }
    public List<Comment> Comments { get; set; } = new();
    public ThreadStatus Status { get; set; }
}