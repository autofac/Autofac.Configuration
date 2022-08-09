// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Autofac.Configuration.Core;
using Autofac.Configuration.Util;
using Autofac.Core;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Autofac.Configuration.Test.Core
{
    public class ConfigurationExtensionsFixture
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void DefaultAssembly_AssemblyNameEmpty(string value)
        {
            var config = SetUpDefaultAssembly(value);
            Assert.Null(config.DefaultAssembly());
        }

        [Fact]
        public void DefaultAssembly_AssemblyNameMissing()
        {
            var config = new ConfigurationBuilder().Build();
            Assert.Null(config.DefaultAssembly());
        }

        [Fact]
        public void DefaultAssembly_AssemblyNotFound()
        {
            var config = SetUpDefaultAssembly("NoSuchAssembly");
            Assert.Throws<FileNotFoundException>(() => config.DefaultAssembly());
        }

        [Fact]
        public void DefaultAssembly_FullAssemblyName()
        {
            var expected = typeof(string).GetTypeInfo().Assembly;
            var config = SetUpDefaultAssembly(expected.FullName);
            Assert.Equal(expected, config.DefaultAssembly());
        }

        [Fact]
        public void DefaultAssembly_NullConfiguration()
        {
            var config = (IConfiguration)null;
            Assert.Throws<ArgumentNullException>(() => config.DefaultAssembly());
        }

        [Fact]
        public void DefaultAssembly_SimpleAssemblyName()
        {
            // String is in a different assembly depending on the
            // target framework. We have to calculate it and truncate
            // the full assembly name at the first comma.
            var expected = typeof(string).GetTypeInfo().Assembly;
            var fullName = expected.FullName.Substring(0, expected.FullName.IndexOf(',', StringComparison.Ordinal));
            var config = SetUpDefaultAssembly(fullName);
            Assert.Equal(expected, config.DefaultAssembly());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void GetAssembly_EmptyKey(string key)
        {
            var config = new ConfigurationBuilder().Build();
            Assert.Throws<ArgumentException>(() => config.GetAssembly(key));
        }

        [Fact]
        public void GetAssembly_NullConfiguration()
        {
            var config = (IConfiguration)null;
            Assert.Throws<ArgumentNullException>(() => config.GetAssembly("defaultAssembly"));
        }

        [Fact]
        public void GetAssembly_NullKey()
        {
            var config = new ConfigurationBuilder().Build();
            Assert.Throws<ArgumentNullException>(() => config.GetAssembly(null));
        }

        [Fact]
        public void GetParameters_ListParameterPopulated()
        {
            var config = EmbeddedConfiguration.LoadJson("ConfigurationExtensions_Parameters.json");
            var component = config.GetSection("components").GetChildren().Where(kvp => kvp["type"] == typeof(HasEnumerableParameter).FullName).First();
            var objectParameter = typeof(HasEnumerableParameter).GetConstructors().First().GetParameters().First(pi => pi.Name == "list");
            var provider = (Func<object>)null;
            var parameter = component.GetParameters("parameters").Cast<Parameter>().FirstOrDefault(rp => rp.CanSupplyValue(objectParameter, new ContainerBuilder().Build(), out provider));
            Assert.NotNull(parameter);
            Assert.NotNull(provider);
            Assert.Equal(new List<string> { "a", "b" }, provider());
        }

        [Fact]
        public void GetParameters_ParameterConversionUsesTypeConverterAttribute()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithJson("ConfigurationExtensions_Parameters.json").Build();
            var obj = container.Resolve<HasConvertibleParametersAndProperties>();
            Assert.NotNull(obj.Parameter);
            Assert.Equal(1.234, obj.Parameter.Value);
        }

        [Theory]
        [MemberData(nameof(GetParameters_SimpleParameters_Source))]
        public void GetParameters_SimpleParameters(string parameterName, object expectedValue)
        {
            var config = EmbeddedConfiguration.LoadJson("ConfigurationExtensions_Parameters.json");
            var component = config.GetSection("components").GetChildren().Where(kvp => kvp["type"] == typeof(HasSimpleParametersAndProperties).FullName).First();
            var objectParameter = typeof(HasSimpleParametersAndProperties).GetConstructors().First().GetParameters().First(pi => pi.Name == parameterName);
            var provider = (Func<object>)null;
            var parameter = component.GetParameters("parameters").Cast<Parameter>().FirstOrDefault(rp => rp.CanSupplyValue(objectParameter, new ContainerBuilder().Build(), out provider));
            Assert.NotNull(parameter);
            Assert.NotNull(provider);
            Assert.Equal(expectedValue, provider());
        }

        [Fact]
        public void GetProperties_DictionaryPropertyEmpty()
        {
            var config = EmbeddedConfiguration.LoadJson("ConfigurationExtensions_Parameters.json");
            var component = config.GetSection("components").GetChildren().Where(kvp => kvp["type"] == typeof(HasDictionaryProperty).FullName).First();
            var property = typeof(HasDictionaryProperty).GetProperty("Empty");
            var provider = (Func<object>)null;
            var parameter = component.GetProperties("properties").Cast<Parameter>().FirstOrDefault(rp => rp.CanSupplyValue(property.SetMethod.GetParameters().First(), new ContainerBuilder().Build(), out provider));

            // Gotcha in ConfigurationModel - if the list/dictionary is empty
            // then configuration won't see it or add the key to the list.
            Assert.Null(parameter);
        }

        [Fact]
        public void GetProperties_DictionaryPropertyPopulated()
        {
            var config = EmbeddedConfiguration.LoadJson("ConfigurationExtensions_Parameters.json");
            var component = config.GetSection("components").GetChildren().Where(kvp => kvp["type"] == typeof(HasDictionaryProperty).FullName).First();
            var property = typeof(HasDictionaryProperty).GetProperty("Populated");
            var provider = (Func<object>)null;
            var parameter = component.GetProperties("properties").Cast<Parameter>().FirstOrDefault(rp => rp.CanSupplyValue(property.SetMethod.GetParameters().First(), new ContainerBuilder().Build(), out provider));
            Assert.NotNull(parameter);
            Assert.NotNull(provider);
            var value = provider();
            Assert.NotNull(value);
            var dict = Assert.IsType<Dictionary<string, double>>(value);
            Assert.Equal(1.234, dict["a"]);
            Assert.Equal(2.345, dict["b"]);
        }

        [Fact]
        public void GetProperties_DictionaryPropertyUsesTypeConverterAttribute()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithJson("ConfigurationExtensions_Parameters.json").Build();
            var obj = container.Resolve<HasDictionaryProperty>();
            Assert.NotNull(obj.Convertible);
            Assert.Equal(2, obj.Convertible.Count);
            Assert.Equal(1.234, obj.Convertible["a"].Value);
            Assert.Equal(2.345, obj.Convertible["b"].Value);
        }

        [Fact]
        public void GetProperties_ListConversionUsesTypeConverterAttribute()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithJson("ConfigurationExtensions_Parameters.json").Build();
            var obj = container.Resolve<HasEnumerableProperty>();
            Assert.NotNull(obj.Convertible);
            var convertible = obj.Convertible.ToArray();
            Assert.Equal(2, convertible.Length);
            Assert.Equal(1.234, convertible[0].Value);
            Assert.Equal(2.345, convertible[1].Value);
        }

        [Fact]
        public void GetProperties_ListPropertyEmpty()
        {
            var config = EmbeddedConfiguration.LoadJson("ConfigurationExtensions_Parameters.json");
            var component = config.GetSection("components").GetChildren().Where(kvp => kvp["type"] == typeof(HasEnumerableProperty).FullName).First();
            var property = typeof(HasEnumerableProperty).GetProperty("Empty");
            var provider = (Func<object>)null;
            var parameter = component.GetProperties("properties").Cast<Parameter>().FirstOrDefault(rp => rp.CanSupplyValue(property.SetMethod.GetParameters().First(), new ContainerBuilder().Build(), out provider));

            // Gotcha in ConfigurationModel - if the list/dictionary is empty
            // then configuration won't see it or add the key to the list.
            Assert.Null(parameter);
        }

        [Fact]
        public void GetProperties_ListPropertyPopulated()
        {
            var config = EmbeddedConfiguration.LoadJson("ConfigurationExtensions_Parameters.json");
            var component = config.GetSection("components").GetChildren().Where(kvp => kvp["type"] == typeof(HasEnumerableProperty).FullName).First();
            var property = typeof(HasEnumerableProperty).GetProperty("Populated");
            var provider = (Func<object>)null;
            var parameter = component.GetProperties("properties").Cast<Parameter>().FirstOrDefault(rp => rp.CanSupplyValue(property.SetMethod.GetParameters().First(), new ContainerBuilder().Build(), out provider));
            Assert.NotNull(parameter);
            Assert.NotNull(provider);
            Assert.Equal(new List<double> { 1.234, 2.345 }, provider());
        }

        [Fact]
        public void GetProperties_PropertyConversionUsesTypeConverterAttribute()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithJson("ConfigurationExtensions_Parameters.json").Build();
            var obj = container.Resolve<HasConvertibleParametersAndProperties>();
            Assert.NotNull(obj.Property);
            Assert.Equal(2.345, obj.Property.Value);
        }

        [Theory]
        [MemberData(nameof(GetProperties_SimpleProperties_Source))]
        public void GetProperties_SimpleProperties(string propertyName, object expectedValue)
        {
            var config = EmbeddedConfiguration.LoadJson("ConfigurationExtensions_Parameters.json");
            var component = config.GetSection("components").GetChildren().Where(kvp => kvp["type"] == typeof(HasSimpleParametersAndProperties).FullName).First();
            var property = typeof(HasSimpleParametersAndProperties).GetProperties().First(pi => pi.Name == propertyName);
            var provider = (Func<object>)null;
            var parameter = component.GetProperties("properties").Cast<Parameter>().FirstOrDefault(rp => rp.CanSupplyValue(property.SetMethod.GetParameters().First(), new ContainerBuilder().Build(), out provider));
            Assert.NotNull(parameter);
            Assert.NotNull(provider);
            Assert.Equal(expectedValue, provider());
        }

        public static IEnumerable<object[]> GetParameters_SimpleParameters_Source()
        {
            yield return new object[] { "number", 1.234 };
            yield return new object[] { "ip", IPAddress.Parse("127.0.0.1") };
        }

        [SuppressMessage("CA1024", "CA1024", Justification = "Data sources must be methods.")]
        public static IEnumerable<object[]> GetProperties_SimpleProperties_Source()
        {
            yield return new object[] { "Text", "text" };
            yield return new object[] { "Url", new Uri("http://localhost") };
        }

        private static IConfiguration SetUpDefaultAssembly(string assemblyName)
        {
            var data = new Dictionary<string, string>
            {
                { "defaultAssembly", assemblyName },
            };
            return new ConfigurationBuilder().AddInMemoryCollection(data).Build();
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through configuration.")]
        private class BaseSimpleParametersAndProperties
        {
            // Issue #2 - Ensure properties in base classes can be set by config.
            public string Text { get; set; }
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through configuration.")]
        private class Convertible
        {
            public double Value { get; set; }
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through configuration.")]
        private class ConvertibleConverter : TypeConverter
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

                if (value is not string str)
                {
                    return base.ConvertFrom(context, culture, value);
                }

                var converter = TypeDescriptor.GetConverter(typeof(double));
                return new Convertible { Value = (double)converter.ConvertFromString(context, culture, str) };
            }
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through configuration.")]
        private class ConvertibleDictionaryConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(ConfiguredDictionaryParameter) || base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value == null)
                {
                    return null;
                }

                if (value is not ConfiguredDictionaryParameter castValue)
                {
                    return base.ConvertFrom(context, culture, value);
                }

                var dict = new Dictionary<string, Convertible>();
                var converter = new ConvertibleConverter();
                foreach (var item in castValue.Dictionary)
                {
                    dict[item.Key] = (Convertible)converter.ConvertFrom(item.Value);
                }

                return dict;
            }
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through configuration.")]
        private class ConvertibleListConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(ConfiguredListParameter) || base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value == null)
                {
                    return null;
                }

                if (value is not ConfiguredListParameter castValue)
                {
                    return base.ConvertFrom(context, culture, value);
                }

                var list = new List<Convertible>();
                var converter = new ConvertibleConverter();
                foreach (string item in castValue.List)
                {
                    list.Add((Convertible)converter.ConvertFrom(item));
                }

                return list;
            }
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through configuration.")]
        private class HasConvertibleParametersAndProperties
        {
            public HasConvertibleParametersAndProperties([TypeConverter(typeof(ConvertibleConverter))] Convertible parameter)
            {
                Parameter = parameter;
            }

            public Convertible Parameter { get; set; }

            [TypeConverter(typeof(ConvertibleConverter))]
            public Convertible Property { get; set; }
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through configuration.")]
        private class HasDictionaryProperty
        {
            [TypeConverter(typeof(ConvertibleDictionaryConverter))]
            public IDictionary<string, Convertible> Convertible { get; set; }

            public Dictionary<string, double> Empty { get; set; }

            public Dictionary<string, double> Populated { get; set; }
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through configuration.")]
        private class HasEnumerableParameter
        {
            public HasEnumerableParameter(IList<string> list)
            {
                List = list;
            }

            public IList<string> List { get; private set; }
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through configuration.")]
        private class HasEnumerableProperty
        {
            [TypeConverter(typeof(ConvertibleListConverter))]
            public IEnumerable<Convertible> Convertible { get; set; }

            public IEnumerable<double> Empty { get; set; }

            public IEnumerable<double> Populated { get; set; }
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through configuration.")]
        private class HasSimpleParametersAndProperties : BaseSimpleParametersAndProperties
        {
            public HasSimpleParametersAndProperties(double number, IPAddress ip)
            {
                Number = number;
                IP = ip;
            }

            public IPAddress IP { get; private set; }

            public double Number { get; private set; }

            public Uri Url { get; set; }
        }
    }
}
