namespace TomLonghurst.PullRequestScanner.Models;

public class TeamMember
{
    public string? DisplayName { get; set; }
    public HashSet<string> UniqueNames { get; } = new();
    public string? Email { get; set; }
    public HashSet<string> Ids { get; } = new();
    public HashSet<string> ImageUrls { get; } = new();

    public string DisplayOrUniqueName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(DisplayName))
            {
                return DisplayName;
            }

            var userName = UniqueNames.FirstOrDefault(u => !string.IsNullOrWhiteSpace(u));
            
            if (userName != null)
            {
                return userName;
            }

            return Email;
        }
    }

    public virtual bool Equals(TeamMember? other)
    {
        return DisplayOrUniqueName == other?.DisplayOrUniqueName;
    }

    public override int GetHashCode()
    {
        return DisplayOrUniqueName.GetHashCode();
    }
}