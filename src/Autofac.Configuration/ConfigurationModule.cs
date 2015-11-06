using Autofac.Configuration.Core;
using Microsoft.Extensions.Configuration;
using System;

namespace Autofac.Configuration
{
    /// <summary>
    /// Module for configuration parsing and registration.
    /// </summary>
    public class ConfigurationModule : Module
    {
        /// <summary>
        /// Gets the configuration to register.
        /// </summary>
        /// <value>
        /// An <see cref="IConfiguration"/> containing the definition for
        /// modules and components to register with the container.
        /// </value>
        public IConfiguration Configuration { get; private set; }

        /// <summary>
        /// Gets or sets the configuration registrar.
        /// </summary>
        /// <value>
        /// An <see cref="Autofac.Configuration.IConfigurationRegistrar"/> that will be used as the
        /// strategy for converting the <see cref="ConfigurationModule.Configuration"/>
        /// into component registrations. If this value is <see langword="null" />, the registrar
        /// will be a <see cref="Autofac.Configuration.Core.ConfigurationRegistrar"/>.
        /// </value>
        public IConfigurationRegistrar ConfigurationRegistrar { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationModule"/> class.
        /// </summary>
        /// <param name="configuration">
        /// An <see cref="IConfiguration"/> containing the definition for
        /// modules and components to register with the container.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="configuration"/> is <see langword="null"/>.
        /// </exception>
        public ConfigurationModule(IConfiguration configuration)
        {
            if(configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            this.Configuration = configuration;
        }

        /// <summary>
        /// Executes the conversion of configuration data into component registrations.
        /// </summary>
        /// <param name="builder">
        /// The <see cref="Autofac.ContainerBuilder"/> into which registrations will be placed.
        /// </param>
        /// <remarks>
        /// <para>
        /// This override uses the <see cref="ConfigurationModule.ConfigurationRegistrar"/>
        /// to convert the <see cref="ConfigurationModule.Configuration"/>
        /// into component registrations in the provided <paramref name="builder" />.
        /// </para>
        /// <para>
        /// If no specific <see cref="ConfigurationModule.ConfigurationRegistrar"/>
        /// is set, the default <see cref="Autofac.Configuration.Core.ConfigurationRegistrar"/> type will be used.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="builder" /> is <see langword="null" />.
        /// </exception>
        protected override void Load(ContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            var registrar = this.ConfigurationRegistrar ?? new ConfigurationRegistrar();
            registrar.RegisterConfiguration(builder, this.Configuration);
        }
    }
}
