# PullRequestScanner
A Github + Azure DevOps Pull Request Scanner that can notify Microsoft Teams of their current statuses (conflicts, outstanding comments, failing checks, etc.)

# Usage
Requires .NET 6

Install via Nuget 
> Install-Package TomLonghurst.PullRequestScanner

In your startup, call: 

```csharp
services
    .AddPullRequestScanner(new PullRequestScannerOptions
    {
        Github = new GithubOrganizationTeamOptions
        {
            PersonalAccessToken = "{username}:{pat}",
            OrganizationSlug = "org-slug",
            TeamSlug = "team-slug"
        },
        AzureDevOps = new AzureDevOpsOptions
        {
            OrganizationSlug = "org-slug",
            TeamSlug = "team-slug",
            PersonalAccessToken = "{username}:{pat}"
        },
        MicrosoftTeams = new MicrosoftTeamsOptions
        {
            WebHookUri = new Uri("https://asos1.webhook.office.com/webhookb2/blahblahblah")
        }
    });
```

Then wherever you want to use it, just inject into your class `IPullRequestScannerNotifier`

E.g. A simple Timed Azure Function to notify your team every morning can look as simple as this:

```csharp

[assembly: WebJobsStartup(typeof(Startup))]
namespace MyNamespace;

public class Startup : IWebJobsStartup
{
    public void Configure(IWebJobsBuilder builder)
    {
        builder.Services
          .AddPullRequestScanner(new PullRequestScannerOptions
          {
              Github = new GithubOrganizationTeamOptions
              {
                  PersonalAccessToken = "{username}:{pat}",
                  OrganizationSlug = "org-slug",
                  TeamSlug = "team-slug"
              },
              AzureDevOps = new AzureDevOpsOptions
              {
                  OrganizationSlug = "org-slug",
                  TeamSlug = "team-slug",
                  PersonalAccessToken = "{username}:{pat}"
              },
              MicrosoftTeams = new MicrosoftTeamsOptions
              {
                  WebHookUri = new Uri("https://asos1.webhook.office.com/webhookb2/blahblahblah")
              }
          });
    }
}

public class MorningTrigger
{
    private readonly IPullRequestScannerNotifier _pullRequestScannerNotifier;

    public MorningTrigger(IPullRequestScannerNotifier pullRequestScannerNotifier)
    {
        _pullRequestScannerNotifier = pullRequestScannerNotifier;
    }
    
    [FunctionName("MorningTrigger")]
    public async Task RunAsync([TimerTrigger("0 0 8 * * 1-5")] TimerInfo myTimer, ILogger log)
    {
        await _pullRequestScannerNotifier.NotifyTeamsChannel(new MicrosoftTeamsPublishOptions());
    }
}
```
