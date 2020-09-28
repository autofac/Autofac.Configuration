// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Configuration;

namespace Autofac.Configuration
{
    /// <summary>
    /// A service for adding configured registrations to a container.
    /// </summary>
    public interface IConfigurationRegistrar
    {
        /// <summary>
        /// Registers the contents of a configuration object into a container builder.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="ContainerBuilder"/> that should receive the configured registrations.
        /// </param>
        /// <param name="configuration">
        /// The <see cref="IConfiguration"/> containing the configured registrations.
        /// </param>
        void RegisterConfiguration(ContainerBuilder builder, IConfiguration configuration);
    }
}
