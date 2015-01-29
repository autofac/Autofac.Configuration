using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Configuration;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Xunit;

namespace Autofac.Tests.Configuration.Core
{
    public class ConfigurationRegistrarFixture
    {
        // TODO: Add tests for dictionary and list parameter parsing.
        /*
[Fact]
public void DictionaryConversionUsesTypeConverterAttribute()
{
    var container = ConfigureContainer();
    var obj = container.Resolve<HasDictionaryProperty>();
    Assert.NotNull(obj.Dictionary);
    Assert.Equal(2, obj.Dictionary.Count);
    Assert.Equal(1, obj.Dictionary["a"].Value);
    Assert.Equal(2, obj.Dictionary["b"].Value);
}

[Fact]
public void ListConversionUsesTypeConverterAttribute()
{
    var container = ConfigureContainer();
    var obj = container.Resolve<HasEnumerableProperty>();
    Assert.NotNull(obj.List);
    Assert.Equal(2, obj.List.Count);
    Assert.Equal(1, obj.List[0].Value);
    Assert.Equal(2, obj.List[1].Value);
}

[Fact]
public void ParameterConversionUsesTypeConverterAttribute()
{
    var container = ConfigureContainer();
    var obj = container.Resolve<HasParametersAndProperties>();
    Assert.NotNull(obj.Parameter);
    Assert.Equal(1, obj.Parameter.Value);
}

[Fact]
public void PropertyConversionUsesTypeConverterAttribute()
{
    var container = ConfigureContainer();
    var obj = container.Resolve<HasParametersAndProperties>();
    Assert.NotNull(obj.Property);
    Assert.Equal(2, obj.Property.Value);
}


        public class Convertible
        {
            public int Value { get; set; }
        }

        public class ConvertibleConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value == null)
                {
                    return null;
                }
                var str = value as String;
                if (str == null)
                {
                    return base.ConvertFrom(context, culture, value);
                }
                var converter = TypeDescriptor.GetConverter(typeof(int));
                return new Convertible { Value = (int)converter.ConvertFromString(context, culture, str) };
            }
        }

        public class ConvertibleListConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(IConfiguration) || base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value == null)
                {
                    return null;
                }
                var castValue = value as IConfiguration;
                if (castValue == null)
                {
                    return base.ConvertFrom(context, culture, value);
                }
                var list = new List<Convertible>();
                var converter = new ConvertibleConverter();
                foreach (var item in castValue.GetSubKey("list").GetSubKeys("item").Select(kvp => kvp.Value.Get("value")))
                {
                    list.Add((Convertible)converter.ConvertFrom(item));
                }
                return list;
            }
        }

        public class ConvertibleDictionaryConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(IConfiguration) || base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value == null)
                {
                    return null;
                }
                var castValue = value as IConfiguration;
                if (castValue == null)
                {
                    return base.ConvertFrom(context, culture, value);
                }
                var dict = new Dictionary<string, Convertible>();
                var converter = new ConvertibleConverter();
                foreach (var item in castValue.GetSubKey("dictionary").GetSubKeys("item"))
                {
                    dict[item.Value.Get("key")] = (Convertible)converter.ConvertFrom(item.Value.Get("value"));
                }
                return dict;
            }
        }

        public class HasDictionaryProperty
        {
            [TypeConverter(typeof(ConvertibleDictionaryConverter))]
            public IDictionary<string, Convertible> Dictionary { get; set; }
        }

        public class HasEnumerableProperty
        {
            [TypeConverter(typeof(ConvertibleListConverter))]
            public IList<Convertible> List { get; set; }
        }

        public class HasParametersAndProperties
        {
            public HasParametersAndProperties([TypeConverter(typeof(ConvertibleConverter))] Convertible parameter) { Parameter = parameter; }

            public Convertible Parameter { get; set; }

            [TypeConverter(typeof(ConvertibleConverter))]
            public Convertible Property { get; set; }
        }
*/




        [Fact]
        public void Load_AllowsMultipleRegistrationsOfSameType()
        {
            var builder = ConfigureContainer("SameTypeRegisteredMultipleTimes");
            var container = builder.Build();
            var collection = container.Resolve<IEnumerable<SimpleComponent>>();
            Assert.Equal(2, collection.Count());

            // Test using Any() because we aren't necessarily guaranteed the order of resolution.
            Assert.True(collection.Any(a => a.Input == 5), "The first registration (5) wasn't found.");
            Assert.True(collection.Any(a => a.Input == 10), "The second registration (10) wasn't found.");
        }

        [Fact]
        public void Load_AllowsMultipleModulesOfSameTypeWithDifferentParameters()
        {
            // Issue #271: Could not register more than one Moudle with the same type but different parameters in XmlConfiguration.
            var builder = ConfigureContainer("SameModuleRegisteredMultipleTimes");
            var container = builder.Build();
            var collection = container.Resolve<IEnumerable<SimpleComponent>>();
            Assert.Equal(2, collection.Count());

            // Test using Any() because we aren't necessarily guaranteed the order of resolution.
            Assert.True(collection.Any(a => a.Message == "First"), "The first registration wasn't found.");
            Assert.True(collection.Any(a => a.Message == "Second"), "The second registration wasn't found.");
        }

        [Fact]
        public void Load_ConstructorInjection()
        {
            var container = ConfigureContainer("SingletonWithTwoServices").Build();
            var cpt = (SimpleComponent)container.Resolve<ITestComponent>();
            Assert.Equal(1, cpt.Input);
        }

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

        [Fact]
        public void Load_LifetimeScope_Singleton()
        {
            var container = ConfigureContainer("SingletonWithTwoServices").Build();
            Assert.Same(container.Resolve<ITestComponent>(), container.Resolve<ITestComponent>());
        }

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

        [Fact]
        public void Load_PropertyInjectionWithProvidedValues()
        {
            var container = ConfigureContainer("SingletonWithTwoServices").Build();
            var cpt = (SimpleComponent)container.Resolve<ITestComponent>();
            Assert.Equal("hello", cpt.Message);
            Assert.True(cpt.ABool, "The Boolean property value was not properly parsed/converted.");
        }

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

        [Fact]
        public void Load_SingleComponentWithTwoServices()
        {
            var container = ConfigureContainer("SingletonWithTwoServices").Build();
            container.AssertRegistered<ITestComponent>("The ITestComponent wasn't registered.");
            container.AssertRegistered<object>("The object wasn't registered.");
            container.AssertNotRegistered<SimpleComponent>("The base SimpleComponent type was incorrectly registered.");
            Assert.Same(container.Resolve<ITestComponent>(), container.Resolve<object>());
        }

        private static ContainerBuilder ConfigureContainer(string configFileBaseName)
        {
            var cb = new ContainerBuilder();
            var fullFilename = "Files/" + configFileBaseName + ".config";
            var csr = new ConfigurationSettingsReader(SectionHandler.DefaultSectionName, fullFilename);
            cb.RegisterModule(csr);
            return cb;
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
