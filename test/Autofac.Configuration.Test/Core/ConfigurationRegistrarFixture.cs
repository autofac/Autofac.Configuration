using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Configuration;
using Autofac.Configuration.Core;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Microsoft.Framework.ConfigurationModel;
using Moq;
using Xunit;

namespace Autofac.Configuration.Test.Core
{
    public class ConfigurationRegistrarFixture
    {
        [Fact]
        public void RegisterConfiguration_NullBuilder()
        {
            var configuration = new Mock<IConfiguration>();
            var registrar = new ConfigurationRegistrar();
            Assert.Throws<ArgumentNullException>(() => registrar.RegisterConfiguration(null, configuration.Object));
        }

        [Fact]
        public void RegisterConfiguration_NullConfiguration()
        {
            var builder = new ContainerBuilder();
            var registrar = new ConfigurationRegistrar();
            Assert.Throws<ArgumentNullException>(() => registrar.RegisterConfiguration(builder, null));
        }

        [Fact]
        public void RegisterConfiguration_AllowsMultipleRegistrationsOfSameType()
        {
            var builder = EmbeddedConfiguration.ConfigureContainerWithJson("SameTypeRegisteredMultipleTimes.json");
            var container = builder.Build();
            var collection = container.Resolve<IEnumerable<SimpleComponent>>();
            Assert.Equal(2, collection.Count());

            // Test using Any() because we aren't necessarily guaranteed the order of resolution.
            Assert.True(collection.Any(a => a.Input == 5), "The first registration (5) wasn't found.");
            Assert.True(collection.Any(a => a.Input == 10), "The second registration (10) wasn't found.");
        }

        [Fact]
        public void RegisterConfiguration_AllowsMultipleModulesOfSameTypeWithDifferentParameters()
        {
            // Issue #271: Could not register more than one module with the same type but different parameters in XmlConfiguration.
            var builder = EmbeddedConfiguration.ConfigureContainerWithJson("SameModuleRegisteredMultipleTimes.json");
            var container = builder.Build();
            var collection = container.Resolve<IEnumerable<SimpleComponent>>();
            Assert.Equal(2, collection.Count());

            // Test using Any() because we aren't necessarily guaranteed the order of resolution.
            Assert.True(collection.Any(a => a.Message == "First"), "The first registration wasn't found.");
            Assert.True(collection.Any(a => a.Message == "Second"), "The second registration wasn't found.");
        }

        [Fact]
        public void RegisterConfiguration_ConstructorInjection()
        {
            var builder = EmbeddedConfiguration.ConfigureContainerWithXml("SingletonWithTwoServices.xml");
            var container = builder.Build();
            var cpt = (SimpleComponent)container.Resolve<ITestComponent>();
            Assert.Equal(1, cpt.Input);
        }

        /*
        [Fact]
        public void Load_ExternalOwnership()
        {
            var container = ConfigureContainer("ExternalOwnership").Build();
            IComponentRegistration registration;
            Assert.True(container.ComponentRegistry.TryGetRegistration(new TypedService(typeof(SimpleComponent)), out registration), "The expected component was not registered.");
            Assert.Equal(InstanceOwnership.ExternallyOwned, registration.Ownership);
        }

        [Fact]
        public void Load_IncludesFileReferences()
        {
            var container = ConfigureContainer("Referrer").Build();
            container.AssertRegisteredNamed<object>("a", "The component from the config file with the specified section name was not registered.");
            container.AssertRegisteredNamed<object>("b", "The component from the config file with the default section name was not registered.");
            container.AssertRegisteredNamed<object>("c", "The component from the referenced raw XML configuration file was not registered.");
        }

        [Fact]
        public void Load_LifetimeScope_InstancePerDependency()
        {
            var container = ConfigureContainer("InstancePerDependency").Build();
            Assert.NotSame(container.Resolve<SimpleComponent>(), container.Resolve<SimpleComponent>());
        }
        */

        [Fact]
        public void RegisterConfiguration_LifetimeScope_Singleton()
        {
            var builder = EmbeddedConfiguration.ConfigureContainerWithXml("SingletonWithTwoServices.xml");
            var container = builder.Build();
            Assert.Same(container.Resolve<ITestComponent>(), container.Resolve<ITestComponent>());
        }

        /*
        [Fact]
        public void Load_MemberOf()
        {
            var builder = ConfigureContainer("MemberOf");
            builder.RegisterCollection<ITestComponent>("named-collection").As<IList<ITestComponent>>();
            var container = builder.Build();
            var collection = container.Resolve<IList<ITestComponent>>();
            var first = collection[0];
            Assert.IsType<SimpleComponent>(first);
        }

        [Fact]
        public void Load_PropertyInjectionEnabledOnComponent()
        {
            var builder = ConfigureContainer("EnablePropertyInjection");
            builder.RegisterType<SimpleComponent>().As<ITestComponent>();
            var container = builder.Build();
            var e = container.Resolve<ComponentConsumer>();
            Assert.NotNull(e.Component);
        }
        */

        [Fact]
        public void RegisterConfiguration_PropertyInjectionWithProvidedValues()
        {
            var builder = EmbeddedConfiguration.ConfigureContainerWithXml("SingletonWithTwoServices.xml");
            var container = builder.Build();
            var cpt = (SimpleComponent)container.Resolve<ITestComponent>();
            Assert.Equal("hello", cpt.Message);
            Assert.True(cpt.ABool, "The Boolean property value was not properly parsed/converted.");
        }

        /*
        [Fact]
        public void Load_AutoActivationEnabledOnComponent()
        {
            var builder = ConfigureContainer("EnableAutoActivation");
            var container = builder.Build();

            IComponentRegistration registration;
            Assert.True(container.ComponentRegistry.TryGetRegistration(new KeyedService("a", typeof(object)), out registration), "The expected component was not registered.");
            Assert.True(registration.Services.Any(a => a.GetType().Name == "AutoActivateService"), "Auto activate service was not registered on the component");
        }

        [Fact]
        public void Load_AutoActivationNotEnabledOnComponent()
        {
            var builder = ConfigureContainer("EnableAutoActivation");
            var container = builder.Build();

            IComponentRegistration registration;
            Assert.True(container.ComponentRegistry.TryGetRegistration(new KeyedService("b", typeof(object)), out registration), "The expected component was not registered.");
            Assert.False(registration.Services.Any(a => a.GetType().Name == "AutoActivateService"), "Auto activate service was registered on the component when it shouldn't be.");
        }

        [Fact]
        public void Load_RegistersMetadata()
        {
            var container = ConfigureContainer("ComponentWithMetadata").Build();
            IComponentRegistration registration;
            Assert.True(container.ComponentRegistry.TryGetRegistration(new KeyedService("a", typeof(object)), out registration), "The expected service wasn't registered.");
            Assert.Equal(42, (int)registration.Metadata["answer"]);
        }
        */

        [Fact]
        public void RegisterConfiguration_SingleComponentWithTwoServices()
        {
            var builder = EmbeddedConfiguration.ConfigureContainerWithXml("SingletonWithTwoServices.xml");
            var container = builder.Build();
            container.AssertRegistered<ITestComponent>("The ITestComponent wasn't registered.");
            container.AssertRegistered<object>("The object wasn't registered.");
            container.AssertNotRegistered<SimpleComponent>("The base SimpleComponent type was incorrectly registered.");
            Assert.Same(container.Resolve<ITestComponent>(), container.Resolve<object>());
        }

        interface ITestComponent { }

        class SimpleComponent : ITestComponent
        {
            public SimpleComponent() { }

            public SimpleComponent(int input) { Input = input; }

            public int Input { get; set; }

            public string Message { get; set; }

            public bool ABool { get; set; }
        }

        class ComponentConsumer
        {
            public ITestComponent Component { get; set; }
        }

        class ParameterizedModule : Module
        {
            public string Message { get; private set; }

            public ParameterizedModule(string message)
            {
                this.Message = message;
            }

            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterType<SimpleComponent>().WithProperty("Message", this.Message);
            }
        }
    }
}
