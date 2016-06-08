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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Autofac.Builder;
using Autofac.Configuration.Util;
using Autofac.Core;
using Microsoft.Extensions.Configuration;

namespace Autofac.Configuration.Core
{
    /// <summary>
    /// Default component configuration processor.
    /// </summary>
    /// <seealso cref="IComponentRegistrar"/>
    public class ComponentRegistrar : IComponentRegistrar
    {
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
        /// You may influence the process by overriding this whole method or by
        /// overriding individual parsing subroutines in this registrar.
        /// </para>
        /// </remarks>
        public virtual void RegisterConfiguredComponents(ContainerBuilder builder, IConfiguration configuration)
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
            foreach (var component in configuration.GetSection("components").GetChildren())
            {
                var registrar = builder.RegisterType(component.GetType("type", defaultAssembly));
                this.RegisterComponentServices(component, registrar, defaultAssembly);
                this.RegisterComponentParameters(component, registrar);
                this.RegisterComponentProperties(component, registrar);
                this.RegisterComponentMetadata(component, registrar, defaultAssembly);
                this.SetLifetimeScope(component, registrar);
                this.SetComponentOwnership(component, registrar);
                this.SetInjectProperties(component, registrar);
                this.SetAutoActivate(component, registrar);
            }
        }

        /// <summary>
        /// Reads configuration data for a component and enumerates the set
        /// of configured services for the component.
        /// </summary>
        /// <param name="component">
        /// The configuration data containing the component. The <c>services</c>
        /// content will be read from this configuration object.
        /// </param>
        /// <param name="defaultAssembly">
        /// The default assembly, if any, from which unqualified type names
        /// should be resolved into types.
        /// </param>
        /// <returns>
        /// An <seealso cref="IEnumerable{T}"/> of <seealso cref="Service"/>
        /// objects associated with the <paramref name="component" />.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="component" /> is <see langword="null" />.
        /// </exception>
        protected virtual IEnumerable<Service> EnumerateComponentServices(IConfiguration component, Assembly defaultAssembly)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            foreach (var serviceDefinition in component.GetSection("services").GetChildren())
            {
                // "name" is a special reserved key in the XML configuration source
                // that enables ordinal collections. To support both JSON and XML
                // sources, we can't use "name" as the keyed service identifier;
                // instead, it must be "key."
                var serviceType = serviceDefinition.GetType("type", defaultAssembly);
                string serviceKey = serviceDefinition["key"];
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

        /// <summary>
        /// Reads configuration data for a component's metadata
        /// and updates the component registration as needed.
        /// </summary>
        /// <param name="component">
        /// The configuration data containing the component. The <c>metadata</c>
        /// content will be read from this configuration object and used
        /// as the metadata values.
        /// </param>
        /// <param name="registrar">
        /// The component registration to update with metadata.
        /// </param>
        /// <param name="defaultAssembly">
        /// The default assembly, if any, from which unqualified type names
        /// should be resolved into types.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="component" /> or <paramref name="registrar" /> is <see langword="null" />.
        /// </exception>
        protected virtual void RegisterComponentMetadata<TReflectionActivatorData, TSingleRegistrationStyle>(IConfiguration component, IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar, Assembly defaultAssembly)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (registrar == null)
            {
                throw new ArgumentNullException(nameof(registrar));
            }

            foreach (var ep in component.GetSection("metadata").GetChildren())
            {
                registrar.WithMetadata(ep["key"], TypeManipulation.ChangeToCompatibleType(ep["value"], ep.GetType("type", defaultAssembly)));
            }
        }

        /// <summary>
        /// Reads configuration data for a component's configured constructor parameter values
        /// and updates the component registration as needed.
        /// </summary>
        /// <param name="component">
        /// The configuration data containing the component. The <c>parameters</c>
        /// content will be read from this configuration object and used
        /// as the properties to inject.
        /// </param>
        /// <param name="registrar">
        /// The component registration to update with constructor parameter injection.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="component" /> or <paramref name="registrar" /> is <see langword="null" />.
        /// </exception>
        protected virtual void RegisterComponentParameters<TReflectionActivatorData, TSingleRegistrationStyle>(IConfiguration component, IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (registrar == null)
            {
                throw new ArgumentNullException(nameof(registrar));
            }

            foreach (var param in component.GetParameters("parameters"))
            {
                registrar.WithParameter(param);
            }
        }

        /// <summary>
        /// Reads configuration data for a component's configured property values
        /// and updates the component registration as needed.
        /// </summary>
        /// <param name="component">
        /// The configuration data containing the component. The <c>properties</c>
        /// content will be read from this configuration object and used
        /// as the properties to inject.
        /// </param>
        /// <param name="registrar">
        /// The component registration to update with property injection.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="component" /> or <paramref name="registrar" /> is <see langword="null" />.
        /// </exception>
        protected virtual void RegisterComponentProperties<TReflectionActivatorData, TSingleRegistrationStyle>(IConfiguration component, IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (registrar == null)
            {
                throw new ArgumentNullException(nameof(registrar));
            }

            foreach (var prop in component.GetProperties("properties"))
            {
                registrar.WithProperty(prop);
            }
        }

        /// <summary>
        /// Reads configuration data for a component's exposed services
        /// and updates the component registration as needed.
        /// </summary>
        /// <param name="component">
        /// The configuration data containing the component. The <c>services</c>
        /// content will be read from this configuration object and used
        /// as the services.
        /// </param>
        /// <param name="registrar">
        /// The component registration to update with services.
        /// </param>
        /// <param name="defaultAssembly">
        /// The default assembly, if any, from which unqualified type names
        /// should be resolved into types.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="component" /> or <paramref name="registrar" /> is <see langword="null" />.
        /// </exception>
        protected virtual void RegisterComponentServices<TReflectionActivatorData, TSingleRegistrationStyle>(IConfiguration component, IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar, Assembly defaultAssembly)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            if (registrar == null)
            {
                throw new ArgumentNullException(nameof(registrar));
            }

            foreach (var service in this.EnumerateComponentServices(component, defaultAssembly))
            {
                registrar.As(service);
            }
        }

        /// <summary>
        /// Sets the auto activation mode for the component.
        /// </summary>
        /// <param name="component">
        /// The configuration data containing the component. The <c>autoActivate</c>
        /// content will be read from this configuration object and used
        /// to determine auto activation status.
        /// </param>
        /// <param name="registrar">
        /// The component registration on which auto activation mode is being set.
        /// </param>
        /// <remarks>
        /// <para>
        /// By default, this implementation understands <see langword="null"/>, empty,
        /// or <see langword="false"/> values (<c>false</c>, <c>0</c>, <c>no</c>)
        /// to mean "not auto-activated" and <see langword="true"/>
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
        /// Thrown if the value for <c>autoActivate</c> is not part of the
        /// recognized grammar.
        /// </exception>
        protected virtual void SetAutoActivate<TReflectionActivatorData, TSingleRegistrationStyle>(IConfiguration component, IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registrar == null)
            {
                throw new ArgumentNullException(nameof(registrar));
            }

            if (component["autoActivate"].ToFlexibleBoolean())
            {
                registrar.AutoActivate();
            }
        }

        /// <summary>
        /// Sets the ownership model for the component.
        /// </summary>
        /// <param name="component">
        /// The configuration data containing the component. The <c>ownership</c>
        /// content will be read from this configuration object and used
        /// to determine component ownership.
        /// </param>
        /// <param name="registrar">
        /// The component registration on which component ownership is being set.
        /// </param>
        /// <remarks>
        /// <para>
        /// By default, this implementation understands <see langword="null"/> or empty
        /// values to be "default ownership model"; <c>lifetime-scope</c> or <c>LifetimeScope</c>
        /// is "owned by lifetime scope"; and <c>external</c> or <c>ExternallyOwned</c> is
        /// "externally owned."
        /// </para>
        /// <para>
        /// By default, this implementation understands the following grammar:
        /// </para>
        /// <list type="table">
        /// <listheader>
        /// <term>Values</term>
        /// <description>Lifetime Scope</description>
        /// </listheader>
        /// <item>
        /// <term><see langword="null"/>, empty</term>
        /// <description>Default - no specified ownership model</description>
        /// </item>
        /// <item>
        /// <term><c>lifetime-scope</c>, <c>LifetimeScope</c></term>
        /// <description>Owned by lifetime scope</description>
        /// </item>
        /// <item>
        /// <term><c>external</c>, <c>ExternallyOwned</c></term>
        /// <description>Externally owned</description>
        /// </item>
        /// </list>
        /// <para>
        /// You may override this method to extend the available grammar for component ownership.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registrar"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the value for <c>ownership</c> is not part of the
        /// recognized grammar.
        /// </exception>
        protected virtual void SetComponentOwnership<TReflectionActivatorData, TSingleRegistrationStyle>(IConfiguration component, IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registrar == null)
            {
                throw new ArgumentNullException(nameof(registrar));
            }

            string ownership = component["ownership"];
            if (string.IsNullOrWhiteSpace(ownership))
            {
                return;
            }

            ownership = ownership.Trim().Replace("-", "");
            if (ownership.Equals("lifetimescope", StringComparison.OrdinalIgnoreCase))
            {
                registrar.OwnedByLifetimeScope();
                return;
            }

            if (ownership.Equals("external", StringComparison.OrdinalIgnoreCase) ||
                ownership.Equals("externallyowned", StringComparison.OrdinalIgnoreCase))
            {
                registrar.ExternallyOwned();
                return;
            }

            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ConfigurationResources.UnrecognisedOwnership, ownership));
        }

        /// <summary>
        /// Sets the property injection mode for the component.
        /// </summary>
        /// <param name="component">
        /// The configuration data containing the component. The <c>injectProperties</c>
        /// content will be read from this configuration object and used
        /// to determine property injection status.
        /// </param>
        /// <param name="registrar">
        /// The component registration on which the property injection mode is being set.
        /// </param>
        /// <remarks>
        /// <para>
        /// By default, this implementation understands <see langword="null"/>, empty,
        /// or <see langword="false"/> values (<c>false</c>, <c>0</c>, <c>no</c>)
        /// to mean "no property injection" and <see langword="true"/>
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
        /// Thrown if the value for <c>injectProperties</c> is not part of the
        /// recognized grammar.
        /// </exception>
        protected virtual void SetInjectProperties<TReflectionActivatorData, TSingleRegistrationStyle>(IConfiguration component, IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registrar == null)
            {
                throw new ArgumentNullException(nameof(registrar));
            }

            if (component["injectProperties"].ToFlexibleBoolean())
            {
                registrar.PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
            }
        }

        /// <summary>
        /// Sets the lifetime scope for the component.
        /// </summary>
        /// <param name="component">
        /// The configuration data containing the component. The <c>instanceScope</c>
        /// content will be read from this configuration object and used
        /// as the lifetime scope.
        /// </param>
        /// <param name="registrar">
        /// The component registration on which the lifetime scope is being set.
        /// </param>
        /// <remarks>
        /// <para>
        /// By default, this implementation understands the following grammar:
        /// </para>
        /// <list type="table">
        /// <listheader>
        /// <term>Values</term>
        /// <description>Lifetime Scope</description>
        /// </listheader>
        /// <item>
        /// <term><see langword="null"/>, empty</term>
        /// <description>Default - no specified scope</description>
        /// </item>
        /// <item>
        /// <term><c>single-instance</c>, <c>SingleInstance</c></term>
        /// <description>Singleton</description>
        /// </item>
        /// <item>
        /// <term><c>instance-per-lifetime-scope</c>, <c>InstancePerLifetimeScope</c>, <c>per-lifetime-scope</c>, <c>PerLifetimeScope</c></term>
        /// <description>One instance per nested lifetime scope</description>
        /// </item>
        /// <item>
        /// <term><c>instance-per-dependency</c>, <c>InstancePerDependency</c>, <c>per-dependency</c>, <c>PerDependency</c></term>
        /// <description>One instance for each resolution call</description>
        /// </item>
        /// <item>
        /// <term><c>instance-per-request</c>, <c>InstancePerRequest</c>, <c>per-request</c>, <c>PerRequest</c></term>
        /// <description>One instance per request lifetime scope</description>
        /// </item>
        /// </list>
        /// <para>
        /// You may override this method to extend the available grammar for lifetime scope.
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registrar"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the value for lifetime scope is not part of the
        /// recognized grammar.
        /// </exception>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "The cyclomatic complexity is in extension methods. This method is actually pretty simple.")]
        protected virtual void SetLifetimeScope<TReflectionActivatorData, TSingleRegistrationStyle>(IConfiguration component, IRegistrationBuilder<object, TReflectionActivatorData, TSingleRegistrationStyle> registrar)
            where TReflectionActivatorData : ReflectionActivatorData
            where TSingleRegistrationStyle : SingleRegistrationStyle
        {
            if (registrar == null)
            {
                throw new ArgumentNullException(nameof(registrar));
            }

            string lifetimeScope = component["instanceScope"];
            if (string.IsNullOrWhiteSpace(lifetimeScope))
            {
                return;
            }

            lifetimeScope = lifetimeScope.Trim().Replace("-", "");
            if (lifetimeScope.Equals("singleinstance", StringComparison.OrdinalIgnoreCase))
            {
                registrar.SingleInstance();
                return;
            }

            if (lifetimeScope.Equals("instanceperlifetimescope", StringComparison.OrdinalIgnoreCase) ||
                lifetimeScope.Equals("perlifetimescope", StringComparison.OrdinalIgnoreCase))
            {
                registrar.InstancePerLifetimeScope();
                return;
            }

            if (lifetimeScope.Equals("instanceperdependency", StringComparison.OrdinalIgnoreCase) ||
                lifetimeScope.Equals("perdependency", StringComparison.OrdinalIgnoreCase))
            {
                registrar.InstancePerDependency();
                return;
            }

            if (lifetimeScope.Equals("instanceperrequest", StringComparison.OrdinalIgnoreCase) ||
                lifetimeScope.Equals("perrequest", StringComparison.OrdinalIgnoreCase))
            {
                registrar.InstancePerRequest();
                return;
            }

            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ConfigurationResources.UnrecognisedScope, lifetimeScope));
        }
    }
}
