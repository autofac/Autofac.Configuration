// This software is part of the Autofac IoC container
// Copyright (c) 2013 Autofac Contributors
// https://autofac.org
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
using System.Linq;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Microsoft.Extensions.Configuration;

namespace Autofac.Configuration.Core
{
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

        private IModule CreateModule(Type type, IConfiguration moduleElement)
        {
            var constructor = GetMostParametersConstructor(type);

            var parametersElement = moduleElement.GetSection("parameters");

            var parameters = constructor.GetParameters()
                .Select(p => parametersElement.GetSection(p.Name).Get(p.ParameterType))
                .ToArray();

            var module = constructor.Invoke(parameters) as IModule;

            var propertiesElement = moduleElement.GetSection("properties");

            propertiesElement.Bind(module);

            return module;
        }

        private static ConstructorInfo GetMostParametersConstructor(Type type)
        {
            var container = new ContainerBuilder().Build();

            var constructors = new DefaultConstructorFinder()
                .FindConstructors(type)
                .Select(c => new ConstructorParameterBinding(c, Enumerable.Empty<Parameter>(), container))
                .ToArray();

            return new MostParametersConstructorSelector()
                .SelectConstructorBinding(constructors, Enumerable.Empty<Parameter>())
                .TargetConstructor;
        }
    }
}
