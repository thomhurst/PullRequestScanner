namespace TomLonghurst.PullRequestScanner.Extensions;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static PullRequestScannerBuilder AddPullRequestScanner(this IServiceCollection services)
    {
        return new PullRequestScannerBuilder(services);
    }
}