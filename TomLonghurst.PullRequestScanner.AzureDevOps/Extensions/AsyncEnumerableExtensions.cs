using System.Collections.Immutable;

namespace TomLonghurst.PullRequestScanner.AzureDevOps.Extensions;

public static class AsyncEnumerableExtensions
{
    public static async Task<ImmutableList<T>> ToImmutableList<T>(this IAsyncEnumerable<T> items,
        CancellationToken cancellationToken = default)
    {
        var results = new List<T>();

        await foreach (var item in items.WithCancellation(cancellationToken)
                           .ConfigureAwait(false))
        {
            results.Add(item);
        }

        return results.ToImmutableList();
    }
}