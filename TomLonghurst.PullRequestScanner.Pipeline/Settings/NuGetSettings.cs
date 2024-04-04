namespace TomLonghurst.PullRequestScanner.Pipeline.Settings;

public record NuGetSettings
{
    public string? ApiKey { get; init; }
}
