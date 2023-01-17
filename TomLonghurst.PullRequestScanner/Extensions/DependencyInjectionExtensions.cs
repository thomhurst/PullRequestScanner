using Microsoft.Extensions.DependencyInjection;

namespace TomLonghurst.PullRequestScanner.Extensions;

public static class DependencyInjectionExtensions
{
    internal static IServiceCollection AddStartupInitializer<TImplementation>(this IServiceCollection services)
        where TImplementation : class, IPullRequestScannerInitializer
    {
        return services.AddTransient<IPullRequestScannerInitializer>(rootServiceProvider =>
        {
            var serviceProvider = rootServiceProvider.CreateAsyncScope().ServiceProvider;
            var startupInitializer = serviceProvider.GetService<TImplementation>();

            if (startupInitializer is null)
            {
                var firstInterface = typeof(TImplementation).GetInterfaces().FirstOrDefault(i => i != typeof(IPullRequestScannerInitializer));
                if (firstInterface != null)
                {
                    startupInitializer = serviceProvider.GetService(firstInterface) as TImplementation;
                }
            }

            if (startupInitializer is null)
            {
                startupInitializer = ActivatorUtilities.CreateInstance<TImplementation>(serviceProvider);
            }

            return startupInitializer;
        });
    }

    public static PullRequestScannerBuilder AddPullRequestScanner(this IServiceCollection services)
    {
        return new PullRequestScannerBuilder(services);
    }
}