// <copyright file="EnumerableExtenions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Extensions;

using TomLonghurst.PullRequestScanner.Models;

public static class EnumerableExtenions
{
    public static List<MutableGroup<TKey, TValue>> ToMutableGrouping<TKey, TValue>(this IEnumerable<IGrouping<TKey, TValue>> grouping)
    {
        return grouping
            .Select(x => new MutableGroup<TKey, TValue>(x.Key, x.ToList()))
            .ToList();
    }
}