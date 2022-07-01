namespace TomLonghurst.PullRequestScanner.Models.Self;

public record Comment
{
    public CommentThread ParentCommentThread { get; set; }
    public Person Author { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
}