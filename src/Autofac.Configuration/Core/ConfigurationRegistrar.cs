// This software is part of the Autofac IoC container
// Copyright (c) 2013 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using Microsoft.Extensions.Configuration;

namespace Autofac.Configuration.Core
{
    /// <summary>
    /// Default service for adding configured registrations to a container.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This default implementation of <see cref="Autofac.Configuration.IConfigurationRegistrar"/>
    /// processes <see cref="IConfiguration"/> contents into registrations for
    /// a <see cref="Autofac.ContainerBuilder"/>. You may derive and override to extend the functionality
    /// or you may implement your own <see cref="Autofac.Configuration.IConfigurationRegistrar"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="Autofac.Configuration.IConfigurationRegistrar"/>
    public class ConfigurationRegistrar : IConfigurationRegistrar
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationRegistrar"/> class.
        /// </summary>
        public ConfigurationRegistrar()
            : this(new ComponentRegistrar(), new ModuleRegistrar())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationRegistrar"/> class.
        /// </summary>
        /// <param name="componentRegistrar">
        /// The <see cref="IComponentRegistrar"/> that will be used to parse
        /// configuration values into component registrations.
        /// </param>
        /// <param name="moduleRegistrar">
        /// The <see cref="IModuleRegistrar"/> that will be used to parse
        /// configuration values into module registrations.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="componentRegistrar" /> or <paramref name="moduleRegistrar" /> is <see langword="null" />.
        /// </exception>
        public ConfigurationRegistrar(IComponentRegistrar componentRegistrar, IModuleRegistrar moduleRegistrar)
        {
            if (componentRegistrar == null)
            {
                throw new ArgumentNullException("componentRegistrar");
            }

            if (moduleRegistrar == null)
            {
                throw new ArgumentNullException("moduleRegistrar");
            }

            this.ComponentRegistrar = componentRegistrar;
            this.ModuleRegistrar = moduleRegistrar;
        }

        /// <summary>
        /// Gets the component registration parser.
        /// </summary>
        /// <value>
        /// The <see cref="IComponentRegistrar"/> that will be used to parse
        /// configuration values into component registrations.
        /// </value>
        public IComponentRegistrar ComponentRegistrar { get; private set; }

        /// <summary>
        /// Gets the module registration parser.
        /// </summary>
        /// <value>
        /// The <see cref="IModuleRegistrar"/> that will be used to parse
        /// configuration values into module registrations.
        /// </value>
        public IModuleRegistrar ModuleRegistrar { get; private set; }

        /// <summary>
        /// Registers the contents of a configuration section into a container builder.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="Autofac.ContainerBuilder"/> that should receive the configured registrations.
        /// </param>
        /// <param name="configuration">
        /// The <see cref="IConfiguration"/> containing the configured registrations.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="builder"/> or <paramref name="configuration"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This method is the primary entry point to configuration section registration. From here,
        /// the various modules, components, and referenced files get registered. You may override
        /// any of those behaviors for a custom registrar if you wish to extend registration behavior.
        /// </para>
        /// </remarks>
        public virtual void RegisterConfiguration(ContainerBuilder builder, IConfiguration configuration)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            this.ModuleRegistrar.RegisterConfiguredModules(builder, configuration);
            this.ComponentRegistrar.RegisterConfiguredComponents(builder, configuration);
        }
    }
}
