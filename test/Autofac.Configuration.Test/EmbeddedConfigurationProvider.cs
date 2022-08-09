// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Autofac.Configuration.Test
{
    /// <summary>
    /// Configuration file proxy provider that skips loading and provides
    /// contnts from a stream.
    /// </summary>
    /// <typeparam name="TSource">
    /// The type of configuration source that generates a file provider.
    /// </typeparam>
    public class EmbeddedConfigurationProvider<TSource> : IConfigurationProvider
        where TSource : IConfigurationSource, new()
    {
        private readonly FileConfigurationProvider _provider;

        public EmbeddedConfigurationProvider(Stream fileStream)
        {
            var source = new TSource();
            _provider = source.Build(new ConfigurationBuilder()) as FileConfigurationProvider;
            _provider.Load(fileStream);
        }

        public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
        {
            return _provider.GetChildKeys(earlierKeys, parentPath);
        }

        public IChangeToken GetReloadToken()
        {
            return _provider.GetReloadToken();
        }

        public void Load()
        {
            // Do nothing - we load via stream.
        }

        public void Set(string key, string value)
        {
            _provider.Set(key, value);
        }

        public bool TryGet(string key, out string value)
        {
            return _provider.TryGet(key, out value);
        }
    }
}
