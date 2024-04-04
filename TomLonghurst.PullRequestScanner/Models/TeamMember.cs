namespace TomLonghurst.PullRequestScanner.Models;

public class TeamMember : IEquatable<TeamMember>
{
    public string? DisplayName { get; set; }
    public List<string> UniqueNames { get; } = new();
    public string? Email { get; set; }
    public List<string> Ids { get; } = new();
    public List<string> ImageUrls { get; } = new();

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

    public string UniqueIdentifier
    {
        get
        {
            var uniqueName = UniqueNames?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

            if (!string.IsNullOrWhiteSpace(uniqueName))
            {
                return uniqueName;
            }

            if (!string.IsNullOrWhiteSpace(Email))
            {
                return Email;
            }

            var id = Ids?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

            if (!string.IsNullOrWhiteSpace(id))
            {
                return id;
            }

            return DisplayName;
        }
    }

    public bool Equals(TeamMember? other)
    {
        if (other is null)
        {
            return false;
        }

        return UniqueIdentifier == other.UniqueIdentifier;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as TeamMember);
    }

    public override int GetHashCode()
    {
        return UniqueIdentifier.GetHashCode();
    }
}