// <copyright file="IHasPlugins.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace TomLonghurst.PullRequestScanner.Services;

using TomLonghurst.PullRequestScanner.Contracts;

public interface IHasPlugins
{
    IEnumerable<IPullRequestPlugin> Plugins { get; }

    TPlugin GetPlugin<TPlugin>()
        where TPlugin : IPullRequestPlugin;
}