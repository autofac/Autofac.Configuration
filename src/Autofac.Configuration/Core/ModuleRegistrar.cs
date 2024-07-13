// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Microsoft.Extensions.Configuration;

namespace Autofac.Configuration.Core;

/// <summary>
/// Default module configuration processor.
/// </summary>
/// <seealso cref="IModuleRegistrar"/>
public class ModuleRegistrar : IModuleRegistrar
{
    /// <summary>
    /// Registers individual configured modules into a container builder.
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
    /// Thrown if there is any issue in parsing the module configuration into registrations.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This is where the individually configured component registrations get added to the <paramref name="builder"/>.
    /// The <c>modules</c> collection from the <paramref name="configuration"/>
    /// get processed into individual modules which are instantiated and activated inside the <paramref name="builder"/>.
    /// </para>
    /// </remarks>
    public virtual void RegisterConfiguredModules(ContainerBuilder builder, IConfiguration configuration)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var defaultAssembly = configuration.DefaultAssembly();
        foreach (var moduleElement in configuration.GetOrderedSubsections("modules"))
        {
            var moduleType = moduleElement.GetType("type", defaultAssembly);

            var module = CreateModule(moduleType, moduleElement);

            builder.RegisterModule(module);
        }
    }

    private static IModule CreateModule(Type type, IConfiguration moduleElement)
    {
        var parametersElement = moduleElement.GetSection("parameters");
        var parameterNames = parametersElement.GetChildren().Select(section => section.Key);

        var constructorFinder = new DefaultConstructorFinder();
        var publicConstructors = constructorFinder.FindConstructors(type);
        if (publicConstructors.Length == 0)
        {
            throw new NoConstructorsFoundException(type, constructorFinder);
        }

        var constructor = GetConstructorMatchingParameterNames(publicConstructors, parameterNames) ?? GetMostParametersConstructor(publicConstructors);
        var parameters = constructor.GetParameters()
                                    .Select(p => parametersElement.GetSection(p.Name).Get(p.ParameterType))
                                    .ToArray();

        var module = (IModule)constructor.Invoke(parameters);

        var propertiesElement = moduleElement.GetSection("properties");

        propertiesElement.Bind(module);

        return module;
    }

    private static ConstructorInfo GetConstructorMatchingParameterNames(ConstructorInfo[] constructors, IEnumerable<string> parameterNames)
    {
        return constructors.Where(constructorInfo => constructorInfo.GetParameters()
                                                                    .Select(pInfo => pInfo.Name)
                                                                    .OrderBy(name => name)
                                                                    .SequenceEqual(parameterNames.OrderBy(s => s), StringComparer.OrdinalIgnoreCase))
                                                                    .FirstOrDefault();
    }

    private static ConstructorInfo GetMostParametersConstructor(ConstructorInfo[] constructors)
    {
        return constructors.OrderByDescending(x => x.GetParameters().Length).First();
    }
}
