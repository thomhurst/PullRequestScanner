namespace TomLonghurst.PullRequestScanner.Models.Self;

public record Approver
{
    public Person Person { get; set; }
    public bool IsRequired { get; set; }
    public Vote Vote { get; set; }
    public DateTimeOffset Time { get; set; }
    public PullRequest PullRequest { get; set; }
}