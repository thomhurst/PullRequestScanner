// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TomLonghurst.PullRequestScanner.AzureDevOps.Extensions;
using TomLonghurst.PullRequestScanner.AzureDevOps.Options;
using TomLonghurst.PullRequestScanner.Enums;
using TomLonghurst.PullRequestScanner.Extensions;
using TomLonghurst.PullRequestScanner.GitHub.Extensions;
using TomLonghurst.PullRequestScanner.GitHub.Options;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Extensions;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Options;
using TomLonghurst.PullRequestScanner.Plugins.MicrosoftTeams.WebHook.Services;
using TomLonghurst.PullRequestScanner.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddPullRequestScanner()
            .AddGithub(new GithubOrganizationTeamOptions()
            {
                OrganizationSlug = "",
                PersonalAccessToken = "",
                RepositoriesToScan = repository => true
            })
            .AddAzureDevOps(new AzureDevOpsOptions()
            {
                OrganizationSlug = "",
                ProjectSlug = "",
                PersonalAccessToken = "",
                RepositoriesToScan = repository => true
            })
            .AddMicrosoftTeamsWebHookPublisher(new MicrosoftTeamsOptions
                {
                    WebHookUri = new Uri("")
                },
                microsoftTeamsWebHookPublisherBuilder =>
                {
                    microsoftTeamsWebHookPublisherBuilder.AddOverviewCardPublisher();
                    microsoftTeamsWebHookPublisherBuilder.AddLeaderboardCardPublisher();
                    microsoftTeamsWebHookPublisherBuilder.AddStatusCardsPublisher(new MicrosoftTeamsStatusPublishOptions
                    {
                        StatusesToPublish = new()
                        {
                            PullRequestStatus.MergeConflicts,
                            PullRequestStatus.ReadyToMerge,
                            PullRequestStatus.FailingChecks,
                            PullRequestStatus.NeedsReviewing,
                            PullRequestStatus.Rejected
                        }
                    });
                });
    })
    .Build();

var pullRequestScanner = host.Services.GetRequiredService<IPullRequestScanner>();
await pullRequestScanner.ExecutePluginsAsync();


// More Granular Control
var pullRequests = await pullRequestScanner.GetPullRequests();
var statusPublisherPlugin = pullRequestScanner.GetPlugin<PullRequestStatusMicrosoftTeamsWebHookPublisher>();
await statusPublisherPlugin.ExecuteAsync(pullRequests, PullRequestStatus.NeedsReviewing);
 