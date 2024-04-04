using Microsoft.Extensions.DependencyInjection;

namespace TomLonghurst.PullRequestScanner.Extensions;

public static class DependencyInjectionExtensions
{
    public static PullRequestScannerBuilder AddPullRequestScanner(this IServiceCollection services)
    {
        return new PullRequestScannerBuilder(services);
    }
}