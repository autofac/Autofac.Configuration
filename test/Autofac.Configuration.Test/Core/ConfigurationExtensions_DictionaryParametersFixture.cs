using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Autofac.Configuration.Test.Core
{
    public class ConfigurationExtensions_DictionaryParametersFixture
    {
        public class A
        {
            public IDictionary<string, string> Dictionary { get; set; }
        }

        [Fact]
        public void InjectsDictionaryProperty()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("ConfigurationExtensions_DictionaryParameters.xml").Build();

            var poco = container.Resolve<A>();

            Assert.True(poco.Dictionary.Count == 2);
            Assert.True(poco.Dictionary.ContainsKey("Key1"));
            Assert.True(poco.Dictionary.ContainsKey("Key2"));
            Assert.Equal("Val1", poco.Dictionary["Key1"]);
            Assert.Equal("Val2", poco.Dictionary["Key2"]);
        }

        public class B
        {
            public IDictionary<string, string> Dictionary { get; set; }

            public B(IDictionary<string, string> dictionary)
            {
                Dictionary = dictionary;
            }
        }

        [Fact]
        public void InjectsDictionaryParameter()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("ConfigurationExtensions_DictionaryParameters.xml").Build();

            var poco = container.Resolve<B>();

            Assert.True(poco.Dictionary.Count == 2);
            Assert.True(poco.Dictionary.ContainsKey("Key1"));
            Assert.True(poco.Dictionary.ContainsKey("Key2"));
            Assert.Equal("Val1", poco.Dictionary["Key1"]);
            Assert.Equal("Val2", poco.Dictionary["Key2"]);
        }

        public class C
        {
            public IDictionary Dictionary { get; set; }
        }

        [Fact]
        public void InjectsNonGenericDictionary()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("ConfigurationExtensions_DictionaryParameters.xml").Build();

            var poco = container.Resolve<C>();

            Assert.True(poco.Dictionary.Count == 2);
            Assert.True(poco.Dictionary.Contains("Key1"));
            Assert.True(poco.Dictionary.Contains("Key2"));
            Assert.Equal("Val1", poco.Dictionary["Key1"]);
            Assert.Equal("Val2", poco.Dictionary["Key2"]);
        }

        public class D
        {
            public Dictionary<string, string> Dictionary { get; set; }
        }

        [Fact]
        public void InjectsConcreteDictionary()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("ConfigurationExtensions_DictionaryParameters.xml").Build();

            var poco = container.Resolve<D>();

            Assert.True(poco.Dictionary.Count == 2);
            Assert.True(poco.Dictionary.ContainsKey("Key1"));
            Assert.True(poco.Dictionary.ContainsKey("Key2"));
            Assert.Equal("Val1", poco.Dictionary["Key1"]);
            Assert.Equal("Val2", poco.Dictionary["Key2"]);
        }

        public class E
        {
            public IDictionary<int, string> Dictionary { get; set; }
        }

        [Fact]
        public void NumericKeysZeroBasedListConvertedToDictionary()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("ConfigurationExtensions_DictionaryParameters.xml").Build();

            var poco = container.Resolve<E>();

            Assert.True(poco.Dictionary.Count == 2);
            Assert.True(poco.Dictionary.ContainsKey(0));
            Assert.True(poco.Dictionary.ContainsKey(1));
            Assert.Equal("Val1", poco.Dictionary[0]);
            Assert.Equal("Val2", poco.Dictionary[1]);
        }

        public class F
        {
            public IDictionary<string, int> Dictionary { get; set; }
        }

        [Fact]
        public void ConvertsDictionaryValue()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("ConfigurationExtensions_DictionaryParameters.xml").Build();

            var poco = container.Resolve<F>();

            Assert.True(poco.Dictionary.Count == 2);
            Assert.True(poco.Dictionary.ContainsKey("Key1"));
            Assert.True(poco.Dictionary.ContainsKey("Key2"));
            Assert.Equal(1, poco.Dictionary["Key1"]);
            Assert.Equal(2, poco.Dictionary["Key2"]);
        }

        public class G
        {
            public IDictionary<int, string> Dictionary { get; set; }
        }

        [Fact]
        public void NumericKeysZeroBasedNonSequential()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("ConfigurationExtensions_DictionaryParameters.xml").Build();

            var poco = container.Resolve<G>();

            Assert.True(poco.Dictionary.Count == 3);
            Assert.True(poco.Dictionary.ContainsKey(0));
            Assert.True(poco.Dictionary.ContainsKey(5));
            Assert.True(poco.Dictionary.ContainsKey(10));
            Assert.Equal("Val0", poco.Dictionary[0]);
            Assert.Equal("Val1", poco.Dictionary[5]);
            Assert.Equal("Val2", poco.Dictionary[10]);
        }
    }
}