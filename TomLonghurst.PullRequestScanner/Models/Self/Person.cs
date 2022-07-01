namespace TomLonghurst.PullRequestScanner.Models.Self;

public record Person
{
    public string UniqueName { get; set; }
    public string DisplayName { get; set; }

    public string DisplayOrUniqueName => string.IsNullOrWhiteSpace(DisplayName) ? UniqueName : DisplayName;

    public virtual bool Equals(Person? other)
    {
        return DisplayOrUniqueName == other?.DisplayOrUniqueName;
    }
}