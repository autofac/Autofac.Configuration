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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Configuration.Util;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Microsoft.Framework.ConfigurationModel;

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

            this.RegisterConfiguredModules(builder, configuration);
            this.RegisterConfiguredComponents(builder, configuration);
        }

        /// <summary>
        /// Loads a type by name.
        /// </summary>
        /// <param name="typeName">
        /// Name of the <see cref="System.Type"/> to load. This may be a partial type name or a fully-qualified type name.
        /// </param>
        /// <param name="defaultAssembly">
        /// The default <see cref="System.Reflection.Assembly"/> to use in type resolution if <paramref name="typeName"/>
        /// is a partial type name.
        /// </param>
        /// <returns>
        /// The resolved <see cref="System.Type"/> based on the specified name.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="typeName"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="typeName"/> is empty.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the specified <paramref name="typeName"/> can't be resolved as a fully-qualified type name and
        /// isn't a partial type name for a <see cref="System.Type"/> found in the <paramref name="defaultAssembly"/>.
        /// </exception>
        protected virtual Type LoadType(string typeName, Assembly defaultAssembly)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            if (typeName.Length == 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, ConfigurationResources.ArgumentMayNotBeEmpty, "type name"), "typeName");
            }
            var type = Type.GetType(typeName);

            if (type == null && defaultAssembly != null)
            {
                type = defaultAssembly.GetType(typeName, false, true); // Don't throw on error; we'll check it later.
            }
            if (type == null)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, ConfigurationResources.TypeNotFound, typeName));
            }
            return type;
        }

        /// <summary>
        /// Registers individual configured components into a container builder.
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
        /// Thrown if there is any issue in parsing the component configuration into registrations.
        /// </exception>
        /// <remarks>
        /// <para>
        /// This is where the individually configured component registrations get added to the <paramref name="builder"/>.
        /// The <c>components</c> collection from the <paramref name="configuration"/>
        /// get processed into individual registrations with associated lifetime scope, name, etc.
        /// </para>
        /// <para>
        /// You may influence the process by overriding this whole method or by overriding these individual
        /// parsing subroutines:
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// <term><see cref="Autofac.Configuration.Core.ConfigurationRegistrar.SetLifetimeScope"/></term>
        /// </item>
        /// <item>
        /// <term><see cref="Autofac.Configuration.Core.ConfigurationRegistrar.SetComponentOwnership"/></term>
        /// </item>
        /// <item>
        /// <term><see cref="Autofac.Configuration.Core.ConfigurationRegistrar.SetInjectProperties"/></term>
        /// </item>
        /// </list>
        /// </remarks>
        protected virtual void RegisterConfiguredComponents(ContainerBuilder builder, IConfiguration configuration)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var defaultAssembly = configuration.DefaultAssembly();
            foreach (var component in configuration.GetSubKey("components").GetSubKeys().Select(kvp => kvp.Value))
            {
                var registrar = builder.RegisterType(LoadType(component.Get("type"), defaultAssembly));

                var services = this.EnumerateComponentServices(component, defaultAssembly);
                foreach (var service in services)
                {
                    registrar.As(service);
                }
                foreach (var param in component.GetParameters("parameters"))
                {
                    registrar.WithParameter(param);
                }
                foreach (var prop in component.GetProperties("properties"))
                {
                    registrar.WithProperty(prop);
                }
                foreach (var ep in component.GetSubKeys("metadata").Select(kvp => kvp.Value))
                {
                    registrar.WithMetadata(ep.Get("key"), TypeManipulation.ChangeToCompatibleType(ep.Get("value"), Type.GetType(ep.Get("type"))));
                }
                if (!string.IsNullOrEmpty(component.Get("memberOf")))
                {
                    registrar.MemberOf(component.Get("memberOf"));
                }
                this.SetLifetimeScope(registrar, component.Get("instanceScope"));
                this.SetComponentOwnership(registrar, component.Get("ownership"));
                this.SetInjectProperties(registrar, component.Get("injectProperties"));
                this.SetAutoActivate(registrar, component.Get("autoActivate"));
            }
        }

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
        protected virtual void RegisterConfiguredModules(ContainerBuilder builder, IConfiguration configuration)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var defaultAssembly = configuration.DefaultAssembly();
            foreach (var moduleElement in configuration.GetSubKey("modules").GetSubKeys().Select(kvp => kvp.Value))
            {
                var moduleType = this.LoadType(moduleElement.Get("type"), defaultAssembly);
                var module = (IModule )null;
                using (var moduleActivator = new ReflectionActivator(
                    moduleType,
                    new DefaultConstructorFinder(),
                    new MostParametersConstructorSelector(),
                    moduleElement.GetParameters("parameters"),
                    moduleElement.GetProperties("properties")))
                {
                    module = (IModule)moduleActivator.ActivateInstance(new ContainerBuilder().Build(), Enumerable.Empty<Parameter>());
                }
                builder.RegisterModule(module);
            }
        }

        /// <summary>
        /// Sets the auto activation mode for the component.
        /// </summary>
        /// <param name="registrar">
        /// The component registration on which auto activation mode is being set.
        /// </param>
        /// <param name="autoActivate">
        /// The <see cref="System.String"/> configuration value associated with auto
        /// activate for this component registration.
        /// </param>
        /// <remarks>
        /// <para>
        /// By default, this implementation understands <see langword="null"/>, empty,
        /// or <see langword="false"/> values (<c>false</c>, <c>0</c>, <c>no</c>)
        /// to mean "no property injection should occur" and <see langword="true"/>
        /// values (<c>true</c>, <c>1</c>, <c>yes</c>) to mean "auto activation
        /// should occur."
        /// </para>
        /// <para>
        /// You may override this method to extend the available grammar for auto activation settings.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registrar"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the value for <paramref name="autoActivate"/> is not part of the
        /// recognized grammar.
        /// </exception>
        protected virtual void SetAutoActivate<TReflectionActivatorData, TSingleRegistrationStyle>(IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar, string autoActivate)
            where TReflectionActivatorData: ReflectionActivatorData
            where TSingleRegistrationStyle: SingleRegistrationStyle
        {
            if (registrar == null)
            {
                throw new ArgumentNullException("registrar");
            }
            if (String.IsNullOrWhiteSpace(autoActivate))
            {
                return;
            }
            switch (autoActivate.Trim().ToUpperInvariant())
            {
                case "NO":
                case "N":
                case "FALSE":
                case "0":
                    break;
                case "YES":
                case "Y":
                case "TRUE":
                case "1":
                    registrar.AutoActivate();
                    break;
                default:
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ConfigurationResources.UnrecognisedAutoActivate, autoActivate));
            }
        }

        /// <summary>
        /// Sets the ownership model for the component.
        /// </summary>
        /// <param name="registrar">
        /// The component registration on which the ownership model is being set.
        /// </param>
        /// <param name="ownership">
        /// The <see cref="System.String"/> configuration value associated with the
        /// ownership model for this component registration.
        /// </param>
        /// <remarks>
        /// <para>
        /// By default, this implementation understands <see langword="null"/> or empty
        /// values to be "default ownership model"; <c>lifetime-scope</c> or <c>LifetimeScope</c>
        /// is "owned by lifetime scope"; and <c>external</c> or <c>ExternallyOwned</c> is
        /// "externally owned."
        /// </para>
        /// <para>
        /// You may override this method to extend the available grammar for component ownership.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registrar"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the value for <paramref name="ownership"/> is not part of the
        /// recognized grammar.
        /// </exception>
        protected virtual void SetComponentOwnership<TReflectionActivatorData, TSingleRegistrationStyle>(IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar, string ownership)
            where TReflectionActivatorData: ReflectionActivatorData
            where TSingleRegistrationStyle: SingleRegistrationStyle
        {
            if (registrar == null)
            {
                throw new ArgumentNullException("registrar");
            }
            if (String.IsNullOrWhiteSpace(ownership))
            {
                return;
            }
            switch (ownership.Trim().ToUpperInvariant())
            {
                case "LIFETIME-SCOPE":
                case "LIFETIMESCOPE":
                    registrar.OwnedByLifetimeScope();
                    break;
                case "EXTERNAL":
                case "EXTERNALLYOWNED":
                    registrar.ExternallyOwned();
                    break;
                default:
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ConfigurationResources.UnrecognisedOwnership, ownership));
            }
        }

        /// <summary>
        /// Sets the property injection mode for the component.
        /// </summary>
        /// <param name="registrar">
        /// The component registration on which property injection mode is being set.
        /// </param>
        /// <param name="injectProperties">
        /// The <see cref="System.String"/> configuration value associated with property
        /// injection for this component registration.
        /// </param>
        /// <remarks>
        /// <para>
        /// By default, this implementation understands <see langword="null"/>, empty,
        /// or <see langword="false"/> values (<c>false</c>, <c>0</c>, <c>no</c>)
        /// to mean "no property injection should occur" and <see langword="true"/>
        /// values (<c>true</c>, <c>1</c>, <c>yes</c>) to mean "property injection
        /// should occur."
        /// </para>
        /// <para>
        /// You may override this method to extend the available grammar for property injection settings.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registrar"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the value for <paramref name="injectProperties"/> is not part of the
        /// recognized grammar.
        /// </exception>
        protected virtual void SetInjectProperties<TReflectionActivatorData, TSingleRegistrationStyle>(IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar, string injectProperties)
            where TReflectionActivatorData: ReflectionActivatorData
            where TSingleRegistrationStyle: SingleRegistrationStyle
        {
            if (registrar == null)
            {
                throw new ArgumentNullException("registrar");
            }
            if (String.IsNullOrWhiteSpace(injectProperties))
            {
                return;
            }
            switch (injectProperties.Trim().ToUpperInvariant())
            {
                case "NO":
                case "N":
                case "FALSE":
                case "0":
                    break;
                case "YES":
                case "Y":
                case "TRUE":
                case "1":
                    registrar.PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
                    break;
                default:
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ConfigurationResources.UnrecognisedInjectProperties, injectProperties));
            }
        }

        /// <summary>
        /// Sets the lifetime scope for the component.
        /// </summary>
        /// <param name="registrar">
        /// The component registration on which the lifetime scope is being set.
        /// </param>
        /// <param name="lifetimeScope">
        /// The <see cref="System.String"/> configuration value associated with the
        /// lifetime scope for this component registration.
        /// </param>
        /// <remarks>
        /// <para>
        /// By default, this implementation understands <see langword="null"/> or empty
        /// values to be "default ownership model"; <c>single-instance</c> or <c>SingleInstance</c>
        /// is singleton; <c>instance-per-lifetime-scope</c>, <c>InstancePerLifetimeScope</c>, <c>per-lifetime-scope</c>,
        /// or <c>PerLifetimeScope</c> is one instance per nested lifetime scope; and <c>instance-per-dependency</c>,
        /// <c>InstancePerDependency</c>, <c>per-dependency</c>, or <c>PerDependency</c> is
        /// one instance for each resolution call.
        /// </para>
        /// <para>
        /// You may override this method to extend the available grammar for lifetime scope.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registrar"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the value for <paramref name="lifetimeScope"/> is not part of the
        /// recognized grammar.
        /// </exception>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "The cyclomatic complexity is in extension methods. This method is actually pretty simple.")]
        protected virtual void SetLifetimeScope<TReflectionActivatorData, TSingleRegistrationStyle>(IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar, string lifetimeScope)
            where TReflectionActivatorData: ReflectionActivatorData
            where TSingleRegistrationStyle: SingleRegistrationStyle
        {
            if (registrar == null)
            {
                throw new ArgumentNullException("registrar");
            }
            if (String.IsNullOrWhiteSpace(lifetimeScope))
            {
                return;
            }
            switch (lifetimeScope.Trim().ToUpperInvariant())
            {
                case "SINGLEINSTANCE":
                case "SINGLE-INSTANCE":
                    registrar.SingleInstance();
                    break;
                case "INSTANCE-PER-LIFETIME-SCOPE":
                case "INSTANCEPERLIFETIMESCOPE":
                case "PER-LIFETIME-SCOPE":
                case "PERLIFETIMESCOPE":
                    registrar.InstancePerLifetimeScope();
                    break;
                case "INSTANCE-PER-DEPENDENCY":
                case "INSTANCEPERDEPENDENCY":
                case "PER-DEPENDENCY":
                case "PERDEPENDENCY":
                    registrar.InstancePerDependency();
                    break;
                case "INSTANCE-PER-REQUEST":
                case "INSTANCEPERREQUEST":
                case "PER-REQUEST":
                case "PERREQUEST":
                    registrar.InstancePerRequest();
                    break;
                default:
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ConfigurationResources.UnrecognisedScope, lifetimeScope));
            }
        }

        private IEnumerable<Service> EnumerateComponentServices(IConfiguration component, Assembly defaultAssembly)
        {
            foreach(var serviceDefinition in component.GetSubKeys("services"))
            {
                // "name" is a special reserved key in the XML configuration source
                // that enables ordinal collections. To support both JSON and XML
                // sources, we can't use "name" as the keyed service identifier;
                // instead, it must be "key."
                var serviceType = LoadType(serviceDefinition.Value.Get("type"), defaultAssembly);
                var serviceKey = serviceDefinition.Value.Get("key");
                if (!string.IsNullOrEmpty(serviceKey))
                {
                    yield return new KeyedService(serviceKey, serviceType);
                }
                else
                {
                    yield return new TypedService(serviceType);
                }
            }
        }
    }
}
