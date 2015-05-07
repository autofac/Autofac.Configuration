using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Autofac.Configuration.Core;
using Autofac.Core;
using Microsoft.Framework.ConfigurationModel;
using Xunit;
using ConfigModel = Microsoft.Framework.ConfigurationModel.Configuration;

namespace Autofac.Tests.Configuration.Core
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
            var config = new ConfigModel();
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
            var expected = typeof(String).GetTypeInfo().Assembly;
            var config = SetUpDefaultAssembly(expected.FullName);
            Assert.Equal(expected, config.DefaultAssembly());
        }

        [Fact]
        public void DefaultAssembly_NullConfiguration()
        {
            IConfiguration config = null;
            Assert.Throws<ArgumentNullException>(() => config.DefaultAssembly());
        }

        [Fact]
        public void DefaultAssembly_SimpleAssemblyName()
        {
            var expected = typeof(String).GetTypeInfo().Assembly;
            var config = SetUpDefaultAssembly("mscorlib");
            Assert.Equal(expected, config.DefaultAssembly());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void GetAssembly_EmptyKey(string key)
        {
            var config = new ConfigModel();
            Assert.Throws<ArgumentException>(() => config.GetAssembly(key));
        }

        [Fact]
        public void GetAssembly_NullConfiguration()
        {
            IConfiguration config = null;
            Assert.Throws<ArgumentNullException>(() => config.GetAssembly("defaultAssembly"));
        }

        [Fact]
        public void GetAssembly_NullKey()
        {
            var config = new ConfigModel();
            Assert.Throws<ArgumentNullException>(() => config.GetAssembly(null));
        }

        [Theory]
        [MemberData("GetParameters_SimpleParameters_Source")]
        public void GetParameters_SimpleParameters(string parameterName, object expectedValue)
        {
            var config = LoadEmbeddedConfig("ConfigurationExtensions_Parameters.json");
            var component = config.GetSubKeys("components").Where(kvp => kvp.Value.Get("type") == typeof(HasSimpleParametersAndProperties).FullName).First().Value;
            var objectParameter = typeof(HasSimpleParametersAndProperties).GetConstructors().First().GetParameters().First(pi => pi.Name == parameterName);
            Func<object> provider = null;
            var parameter = component.GetParameters("parameters").Cast<Parameter>().FirstOrDefault(rp => rp.CanSupplyValue(objectParameter, new ContainerBuilder().Build(), out provider));
            Assert.NotNull(parameter);
            Assert.NotNull(provider);
            Assert.Equal(expectedValue, provider());
        }

        [Theory]
        [MemberData("GetProperties_SimpleProperties_Source")]
        public void GetProperties_SimpleProperties(string propertyName, object expectedValue)
        {
            var config = LoadEmbeddedConfig("ConfigurationExtensions_Parameters.json");
            var component = config.GetSubKeys("components").Where(kvp => kvp.Value.Get("type") == typeof(HasSimpleParametersAndProperties).FullName).First().Value;
            var property = typeof(HasSimpleParametersAndProperties).GetProperties().First(pi => pi.Name == propertyName);
            Func<object> provider = null;
            var parameter = component.GetProperties("properties").Cast<Parameter>().FirstOrDefault(rp => rp.CanSupplyValue(property.SetMethod.GetParameters().First(), new ContainerBuilder().Build(), out provider));
            Assert.NotNull(parameter);
            Assert.NotNull(provider);
            Assert.Equal(expectedValue, provider());
        }

        public static IEnumerable<object[]> GetProperties_SimpleProperties_Source
        {
            get
            {
                yield return new object[] { "Text", "text" };
                yield return new object[] { "Url", new Uri("http://localhost") };
            }
        }

        public static IEnumerable<object[]> GetParameters_SimpleParameters_Source
        {
            get
            {
                yield return new object[] { "number", 1 };
                yield return new object[] { "ip", IPAddress.Parse("127.0.0.1") };
            }
        }

        private static IConfiguration LoadEmbeddedConfig(string configFile)
        {
            var config = new ConfigModel();
            var source = new JsonConfigurationSource("path", true);
            using (var stream = typeof(ConfigurationExtensionsFixture).GetTypeInfo().Assembly.GetManifestResourceStream("Files/" + configFile))
            {
                typeof(JsonConfigurationSource).GetMethod("Load", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(source, new object[] { stream });
            }
            config.Add(source, false);
            return config;
        }

        private static IConfiguration SetUpDefaultAssembly(string assemblyName)
        {
            var source = new MemoryConfigurationSource();
            source.Add("defaultAssembly", assemblyName);
            var config = new ConfigModel();
            config.Add(source);
            return config;
        }

        public class HasSimpleParametersAndProperties
        {
            public HasSimpleParametersAndProperties(int number, IPAddress ip)
            {
                this.Number = number;
                this.IP = ip;
            }

            public int Number { get; private set; }

            public IPAddress IP { get; private set; }

            public string Text { get; set; }

            public Uri Url { get; set; }
        }
    }
}
