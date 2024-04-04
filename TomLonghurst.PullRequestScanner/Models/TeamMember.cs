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
            if (!string.IsNullOrWhiteSpace(this.DisplayName))
            {
                return this.DisplayName;
            }

            var userName = this.UniqueNames.FirstOrDefault(u => !string.IsNullOrWhiteSpace(u));

            if (userName != null)
            {
                return userName;
            }

            return this.Email;
        }
    }

    public string UniqueIdentifier
    {
        get
        {
            var uniqueName = this.UniqueNames?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

            if (!string.IsNullOrWhiteSpace(uniqueName))
            {
                return uniqueName;
            }

            if (!string.IsNullOrWhiteSpace(this.Email))
            {
                return this.Email;
            }

            var id = this.Ids?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

            if (!string.IsNullOrWhiteSpace(id))
            {
                return id;
            }

            return this.DisplayName;
        }
    }

    public bool Equals(TeamMember? other)
    {
        if (other is null)
        {
            return false;
        }

        return this.UniqueIdentifier == other.UniqueIdentifier;
    }

    public override bool Equals(object? obj)
    {
        return this.Equals(obj as TeamMember);
    }

    public override int GetHashCode()
    {
        return this.UniqueIdentifier.GetHashCode();
    }
}