using TomLonghurst.PullRequestScanner.Models;

namespace TomLonghurst.PullRequestScanner.Extensions;

internal static class EnumerableExtenions
{
    public static List<MutableGroup<TKey, TValue>> ToMutableGrouping<TKey, TValue>(this IEnumerable<IGrouping<TKey, TValue>> grouping)
    {
        return grouping
            .Select(x => new MutableGroup<TKey, TValue>(x.Key, x.ToList()))
            .ToList();
    }
}