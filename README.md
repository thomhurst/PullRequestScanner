# PullRequestScanner

A Pull Request Scanner that is extendable, allowing you to add pull request providers, or plugins, that do something with the pull request data.
If you write the implementation for a new provider, or a new plugin, feel free to open a pull request to add it into the repository.

Currently the out-of-the-box providers are Azure DevOps and GitHub, with a plugin that can publish cards to a Microsoft Teams Webhook, notifying your team of any open pull requests and their current state.

# Install

Install via Nuget 

> Install-Package TomLonghurst.PullRequestScanner

## Available Providers

### Azure DevOps

> Install-Package TomLonghurst.PullRequestScanner.AzureDevOps

### GitHub

> Install-Package TomLonghurst.PullRequestScanner.GitHub

## Available Plugins

### Microsoft Teams Web Hook

> Install-Package TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook

## Usage

Requires .NET 6

In your startup, call: 

```csharp
services
    .AddPullRequestScanner();
```

And either use any of the already built providers and plugins, or build your own and plug them in.
That could either look like:

```csharp
        builder.Services
            .AddPullRequestScanner()
            .AddGithub(new GithubOrganizationTeamOptions
            {
                PersonalAccessToken = myGithubPat,
                OrganizationSlug = myOrganisation,
                TeamSlug = myGithubTeamSlug
            })
            .AddAzureDevOps(new AzureDevOpsOptions
            {
                OrganizationSlug = myOrganisation,
                ProjectSlug = myAzureDevOpsProjectSlug,
                PersonalAccessToken = myAzureDevopsPat
            })
            .AddMicrosoftTeamsWebHookPublisher(new MicrosoftTeamsOptions
                {
                    WebHookUri = new Uri(myMicrosoftTeamsWebhookUri)
                },
                microsoftTeamsWebHookPublisherBuilder =>
                {
                    microsoftTeamsWebHookPublisherBuilder.AddLeaderboardCardPublisher();
                    microsoftTeamsWebHookPublisherBuilder.AddOverviewCardPublisher();
                    microsoftTeamsWebHookPublisherBuilder.AddStatusCardsPublisher();
                }
            );
```

or

```csharp
        builder.Services
            .AddPullRequestScanner()
            .AddPullRequestProvider(serviceProvider => serviceProvider.GetRequiredService<MyCustomPullRequestProvider1>())
            .AddPullRequestProvider(serviceProvider => serviceProvider.GetRequiredService<MyCustomPullRequestProvider2>())
            .AddPullRequestProvider(serviceProvider => serviceProvider.GetRequiredService<MyCustomPullRequestProvider3>())
            .AddPlugin(serviceProvider => serviceProvider.GetRequiredService<MyCustomPlugin1>())
            .AddPlugin(serviceProvider => serviceProvider.GetRequiredService<MyCustomPlugin2>());
```

Then wherever you want to use it, just inject `IPullRequestScanner` into your class. With this you can:

-   Get Pull Request Data
-   Execute All Plugins
-   Execute All Plugins that meet a condition
-   Get a particular plugin and invoke it with more granular control

## Example

A simple Timed Azure Function to notify your team every morning can look as simple as this:

```csharp

[assembly: WebJobsStartup(typeof(Startup))]
namespace MyNamespace;

public class MorningTrigger
{
    private readonly IPullRequestScanner _pullRequestScanner;

    public MorningTrigger(IPullRequestScanner pullRequestScanner)
    {
        _pullRequestScanner = pullRequestScanner;
    }

    [FunctionName("MorningTrigger")]
    public async Task RunAsync([TimerTrigger("0 0 8 * * 1-5")] TimerInfo myTimer, ILogger log)
    {
        await _pullRequestScanner.ExecutePluginsAsync();
    }
}
```

If I want more control over my plugins or pull request data, I could do this:

```csharp
public class MorningTrigger
{
    private readonly IPullRequestScanner _pullRequestScanner;
    private readonly PullRequestOverviewMicrosoftTeamsWebHookPublisher _overviewCardPublisher;
    private readonly PullRequestStatusMicrosoftTeamsWebHookPublisher _statusCardPublisher;

    public MorningTrigger(IPullRequestScanner pullRequestScanner)
    {
        _pullRequestScanner = pullRequestScanner;
        _overviewCardPublisher = _pullRequestScanner.GetPlugin<PullRequestOverviewMicrosoftTeamsWebHookPublisher>();
        _statusCardPublisher = _pullRequestScanner.GetPlugin<PullRequestStatusMicrosoftTeamsWebHookPublisher>();
    }
    
    [FunctionName("MorningTrigger")]
    public async Task RunAsync([TimerTrigger("0 0 8 * * 1-5")] TimerInfo myTimer, ILogger log)
    {
        var pullRequests = await _pullRequestScanner.GetPullRequests();
        
        await _overviewCardPublisher.ExecuteAsync(pullRequests);
        
        foreach (var pullRequestStatus in new[] { PullRequestStatus.MergeConflicts, PullRequestStatus.ReadyToMerge })
        {
            await _statusCardPublisher.ExecuteAsync(pullRequests, pullRequestStatus);
        }
    }
}
```

## Example Output

![image](https://user-images.githubusercontent.com/30480171/213030165-b16dc756-afdc-4a3b-993d-c3d9a78c4dc1.png)

![image](https://user-images.githubusercontent.com/30480171/213030213-ba01394d-e5ae-4960-b519-b3039e2eacfe.png)

![image](https://user-images.githubusercontent.com/30480171/213030243-a75b6ff3-5e26-408c-b553-6ede840976d0.png)

![image](https://user-images.githubusercontent.com/30480171/213030268-0b4c5325-3539-44f4-a842-d625cef64ce6.png)
