using Microsoft.Extensions.Configuration;

namespace Autofac.Configuration
{
    /// <summary>
    /// Defines a registration mechanism that converts configuration data
    /// into Autofac module registrations.
    /// </summary>
    /// <seealso cref="Autofac.Configuration.Core.ModuleRegistrar"/>
    public interface IModuleRegistrar
    {
        /// <summary>
        /// Registers individual configured modules into a container builder.
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
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if there is any issue in parsing the module configuration into registrations.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Implementations of this method are responsible for adding modules
        /// to the container by parsing configuration model data and executing
        /// the registration logic.
        /// </para>
        /// </remarks>
        void RegisterConfiguredModules(ContainerBuilder builder, IConfiguration configuration);
    }
}
