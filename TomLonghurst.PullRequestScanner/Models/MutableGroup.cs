// <copyright file="MutableGroup.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Models;

public class MutableGroup<TKey, TValue>
{
    public MutableGroup(TKey key, IEnumerable<TValue> values)
    {
        this.Key = key;
        this.Values = values.ToList();
    }

    public TKey Key { get; }

    public List<TValue> Values { get; }
}