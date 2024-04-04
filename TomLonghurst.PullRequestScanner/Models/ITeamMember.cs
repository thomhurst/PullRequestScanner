namespace TomLonghurst.PullRequestScanner.Models;

public interface ITeamMember
{
    string DisplayName { get; }

    string UniqueName { get; }

    string Id { get; }

    string Email { get; }

    string ImageUrl { get; }
}