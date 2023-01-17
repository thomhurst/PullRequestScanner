using TomLonghurst.PullRequestScanner.Services;

namespace TomLonghurst.PullRequestScanner.Models;

public record Comment
{
    public CommentThread ParentCommentThread { get; set; }
    public TeamMember Author { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}