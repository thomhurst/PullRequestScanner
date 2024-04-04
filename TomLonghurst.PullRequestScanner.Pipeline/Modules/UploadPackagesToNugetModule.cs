using EnumerableAsyncProcessor.Extensions;
using TomLonghurst.PullRequestScanner.Pipeline.Settings;
using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Models;
using ModularPipelines.Modules;

namespace TomLonghurst.PullRequestScanner.Pipeline.Modules;

[DependsOn<RunUnitTestsModule>]
[DependsOn<PackagePathsParserModule>]
public class UploadPackagesToNugetModule : Module<CommandResult[]>
{
    private readonly IOptions<NuGetSettings> _options;

    public UploadPackagesToNugetModule(IOptions<NuGetSettings> options)
    {
        _options = options;
    }

    protected override async Task<SkipDecision> ShouldSkip(IPipelineContext context)
    {
        await Task.Yield();
        var publishPackages =
            context.Environment.EnvironmentVariables.GetEnvironmentVariable("PUBLISH_PACKAGES")!;

        if (!bool.TryParse(publishPackages, out var shouldPublishPackages) 
            || !shouldPublishPackages)
        {
            return SkipDecision.Skip("User hasn't selected to publish");
        }

        return SkipDecision.DoNotSkip;
    }

    protected override async Task<CommandResult[]?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(_options.Value.ApiKey);

        var packagePaths = await GetModule<PackagePathsParserModule>();

        return await packagePaths.Value!
            .SelectAsync(async nugetFile => await context.DotNet().Nuget.Push(new DotNetNugetPushOptions
            {
                Path = nugetFile,
                Source = "https://api.nuget.org/v3/index.json",
                ApiKey = _options.Value.ApiKey!,
            }, cancellationToken), cancellationToken: cancellationToken)
            .ProcessOneAtATime();
    }
}
