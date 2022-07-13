using System.Runtime.CompilerServices;

namespace TomLonghurst.PullRequestScanner.Services;

public record TeamMember
{
    public string? DisplayName { get; set; }
    public string? GithubUsername { get; set; }
    public string? DevOpsUsername { get; set; }
    public string? Email { get; set; }
    public string GithubId { get; set; }
    public string DevOpsId { get; set; }
    public string? GithubImageUrl { get; set; }
    public string? DevOpsImageUrl { get; set; }

    public string DisplayOrUniqueName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(DisplayName))
            {
                return DisplayName;
            }
            
            if (!string.IsNullOrWhiteSpace(DevOpsUsername))
            {
                return DevOpsUsername;
            }
            
            if (!string.IsNullOrWhiteSpace(GithubUsername))
            {
                return GithubUsername;
            }

            return Email;
        }
    }

    public virtual bool Equals(TeamMember? other)
    {
        return DisplayOrUniqueName == other?.DisplayOrUniqueName;
    }
}