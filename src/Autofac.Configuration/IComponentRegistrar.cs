﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.Configuration;

namespace Autofac.Configuration;

/// <summary>
/// Defines a registration mechanism that converts configuration data
/// into Autofac component registrations.
/// </summary>
/// <seealso cref="Core.ComponentRegistrar"/>
public interface IComponentRegistrar
{
    /// <summary>
    /// Registers individual configured components into a container builder.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="ContainerBuilder"/> that should receive the configured registrations.
    /// </param>
    /// <param name="configuration">
    /// The <see cref="IConfiguration"/> containing the configured registrations.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="builder"/> or <paramref name="configuration"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if there is any issue in parsing the component configuration into registrations.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Implementations of this method are responsible for adding components
    /// to the container by parsing configuration model data and executing
    /// the registration logic.
    /// </para>
    /// </remarks>
    void RegisterConfiguredComponents(ContainerBuilder builder, IConfiguration configuration);
}
