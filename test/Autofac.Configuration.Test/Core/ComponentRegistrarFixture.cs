﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Xunit;

namespace Autofac.Configuration.Test.Core
{
    public class ComponentRegistrarFixture
    {
        [Fact]
        public void RegisterConfiguredComponents_AllowsMultipleRegistrationsOfSameType()
        {
            var builder = EmbeddedConfiguration.ConfigureContainerWithJson("ComponentRegistrar_SameTypeRegisteredMultipleTimes.json");
            var container = builder.Build();
            var collection = container.Resolve<IEnumerable<SimpleComponent>>();
            Assert.Equal(2, collection.Count());

            // Test using Any() because we aren't necessarily guaranteed the order of resolution.
            Assert.True(collection.Any(a => a.Input == 5.123), "The first registration (5.123) wasn't found.");
            Assert.True(collection.Any(a => a.Input == 10.234), "The second registration (10.234) wasn't found.");
        }

        [Fact]
        public void RegisterConfiguredComponents_AutoActivationEnabledOnComponent()
        {
            var builder = EmbeddedConfiguration.ConfigureContainerWithJson("ComponentRegistrar_EnableAutoActivation.json");
            var container = builder.Build();
            Assert.True(container.ComponentRegistry.TryGetRegistration(new KeyedService("a", typeof(object)), out IComponentRegistration registration), "The expected component was not registered.");
            Assert.True(registration.Services.Any(a => a.GetType().Name == "AutoActivateService"), "Auto activate service was not registered on the component");
        }

        [Fact]
        public void RegisterConfiguredComponents_AutoActivationNotEnabledOnComponent()
        {
            var builder = EmbeddedConfiguration.ConfigureContainerWithJson("ComponentRegistrar_EnableAutoActivation.json");
            var container = builder.Build();
            Assert.True(container.ComponentRegistry.TryGetRegistration(new KeyedService("b", typeof(object)), out IComponentRegistration registration), "The expected component was not registered.");
            Assert.False(registration.Services.Any(a => a.GetType().Name == "AutoActivateService"), "Auto activate service was registered on the component when it shouldn't be.");
        }

        [Fact]
        public void RegisterConfiguredComponents_ConstructorInjection()
        {
            var builder = EmbeddedConfiguration.ConfigureContainerWithXml("ComponentRegistrar_SingletonWithTwoServices.xml");
            var container = builder.Build();
            var cpt = (SimpleComponent)container.Resolve<ITestComponent>();
            Assert.Equal(1.234, cpt.Input);
        }

        [Fact]
        public void RegisterConfiguredComponents_ExternalOwnership()
        {
            var builder = EmbeddedConfiguration.ConfigureContainerWithJson("ComponentRegistrar_ExternalOwnership.json");
            var container = builder.Build();
            Assert.True(container.ComponentRegistry.TryGetRegistration(new TypedService(typeof(SimpleComponent)), out IComponentRegistration registration), "The expected component was not registered.");
            Assert.Equal(InstanceOwnership.ExternallyOwned, registration.Ownership);
        }

        [Fact]
        public void RegisterConfiguredComponents_LifetimeScope_InstancePerDependency()
        {
            var builder = EmbeddedConfiguration.ConfigureContainerWithJson("ComponentRegistrar_InstancePerDependency.json");
            var container = builder.Build();
            Assert.NotSame(container.Resolve<SimpleComponent>(), container.Resolve<SimpleComponent>());
        }

        [Fact]
        public void RegisterConfiguredComponents_LifetimeScope_Singleton()
        {
            var builder = EmbeddedConfiguration.ConfigureContainerWithXml("ComponentRegistrar_SingletonWithTwoServices.xml");
            var container = builder.Build();
            Assert.Same(container.Resolve<ITestComponent>(), container.Resolve<ITestComponent>());
        }

        [Fact]
        public void RegisterConfiguredComponents_PropertyInjectionEnabledOnComponent()
        {
            var builder = EmbeddedConfiguration.ConfigureContainerWithJson("ComponentRegistrar_EnablePropertyInjection.json");
            builder.RegisterType<SimpleComponent>().As<ITestComponent>();
            builder.RegisterInstance("hello").As<string>();
            var container = builder.Build();
            var e = container.Resolve<ComponentConsumer>();
            Assert.NotNull(e.Component);

            // Issue #2 - Ensure properties in base classes can be set by config.
            Assert.Equal("hello", e.Message);
        }

        [Fact]
        public void RegisterConfiguredComponents_PropertyInjectionWithProvidedValues()
        {
            var builder = EmbeddedConfiguration.ConfigureContainerWithXml("ComponentRegistrar_SingletonWithTwoServices.xml");
            var container = builder.Build();
            var cpt = (SimpleComponent)container.Resolve<ITestComponent>();
            Assert.True(cpt.ABool, "The Boolean property value was not properly parsed/converted.");

            // Issue #2 - Ensure properties in base classes can be set by config.
            Assert.Equal("hello", cpt.Message);
        }

        [Fact]
        public void RegisterConfiguredComponents_RegistersMetadata()
        {
            var builder = EmbeddedConfiguration.ConfigureContainerWithJson("ComponentRegistrar_ComponentWithMetadata.json");
            var container = builder.Build();
            Assert.True(container.ComponentRegistry.TryGetRegistration(new KeyedService("a", typeof(object)), out IComponentRegistration registration), "The expected service wasn't registered.");
            Assert.Equal(42.42, (double)registration.Metadata["answer"]);
        }

        [Fact]
        public void RegisterConfiguredComponents_SingleComponentWithTwoServices()
        {
            var builder = EmbeddedConfiguration.ConfigureContainerWithXml("ComponentRegistrar_SingletonWithTwoServices.xml");
            var container = builder.Build();
            container.AssertRegistered<ITestComponent>("The ITestComponent wasn't registered.");
            container.AssertRegistered<object>("The object wasn't registered.");
            container.AssertNotRegistered<SimpleComponent>("The base SimpleComponent type was incorrectly registered.");
            Assert.Same(container.Resolve<ITestComponent>(), container.Resolve<object>());
        }

        [Fact]
        public void RegisterConfiguredComponents_ComponentsMissingName()
        {
            var builder = EmbeddedConfiguration.ConfigureContainerWithXml("ComponentRegistrar_ComponentsMissingName.xml");
            var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());
            Assert.Equal("The 'components' collection should be ordinal (like an array) with items that have numeric names to indicate the index in the collection. 'components' didn't have a numeric name so couldn't be parsed. Check https://autofac.readthedocs.io/en/latest/configuration/xml.html for configuration examples.", exception.Message);
        }

        [Fact]
        public void RegisterConfiguredComponents_ServicesMissingName()
        {
            var builder = EmbeddedConfiguration.ConfigureContainerWithXml("ComponentRegistrar_ServicesMissingName.xml");
            var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());
            Assert.Equal("The 'services' collection should be ordinal (like an array) with items that have numeric names to indicate the index in the collection. 'components:0:services' didn't have a numeric name so couldn't be parsed. Check https://autofac.readthedocs.io/en/latest/configuration/xml.html for configuration examples.", exception.Message);
        }

        [Fact]
        public void RegisterConfiguredComponents_MetadataMissingName()
        {
            var builder = EmbeddedConfiguration.ConfigureContainerWithXml("ComponentRegistrar_MetadataMissingName.xml");
            var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());
            Assert.Equal("The 'metadata' collection should be ordinal (like an array) with items that have numeric names to indicate the index in the collection. 'components:0:metadata' didn't have a numeric name so couldn't be parsed. Check https://autofac.readthedocs.io/en/latest/configuration/xml.html for configuration examples.", exception.Message);
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through configuration.")]
        private class ComponentConsumer : BaseComponentConsumer
        {
            public ITestComponent Component { get; set; }
        }

        private class BaseComponentConsumer
        {
            // Issue #2 - Ensure properties in base classes can be set by config.
            public string Message { get; set; }
        }

        private interface ITestComponent
        {
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through configuration.")]
        private class SimpleComponent : BaseComponent, ITestComponent
        {
            public SimpleComponent()
            {
            }

            public SimpleComponent(double input)
            {
                Input = input;
            }

            public bool ABool { get; set; }

            public double Input { get; set; }
        }

        private class BaseComponent
        {
            // Issue #2 - Ensure properties in base classes can be set by config.
            public string Message { get; set; }
        }
    }
}
