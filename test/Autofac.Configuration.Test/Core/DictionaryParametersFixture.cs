using System.Collections;
using System.Collections.Generic;
using Autofac.Configuration;
using Xunit;
using System;

namespace Autofac.Configuration.Test.Core
{
    public class DictionaryParametersFixture
    {
        public class A
        {
            public IDictionary<string, string> Dictionary { get; set; }
        }

        [Fact]
        public void InjectsDictionaryProperty()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("DictionaryParameters.xml").Build();

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
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("DictionaryParameters.xml").Build();

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
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("DictionaryParameters.xml").Build();

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
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("DictionaryParameters.xml").Build();

            var poco = container.Resolve<D>();

            Assert.True(poco.Dictionary.Count == 2);
            Assert.True(poco.Dictionary.ContainsKey("Key1"));
            Assert.True(poco.Dictionary.ContainsKey("Key2"));
            Assert.Equal("Val1", poco.Dictionary["Key1"]);
            Assert.Equal("Val2", poco.Dictionary["Key2"]);
        }

        public class E
        {
            public IDictionary<Guid, string> Dictionary { get; set; }
        }

        [Fact]
        public void ConvertsDictionaryKey()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("DictionaryParameters.xml").Build();

            var poco = container.Resolve<E>();

            var key1 = new Guid("94be24db-22a1-45b7-8dbc-a09ffc11468d");
            var key2 = new Guid("cf735002-8f21-4b72-a31f-1de6e24930bc");
            Assert.True(poco.Dictionary.Count == 2);
            Assert.True(poco.Dictionary.ContainsKey(key1));
            Assert.True(poco.Dictionary.ContainsKey(key2));
            Assert.Equal("Val1", poco.Dictionary[key1]);
            Assert.Equal("Val2", poco.Dictionary[key2]);
        }

        public class F
        {
            public IDictionary<string, int> Dictionary { get; set; }
        }

        [Fact]
        public void ConvertsDictionaryValue()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("DictionaryParameters.xml").Build();

            var poco = container.Resolve<F>();

            Assert.True(poco.Dictionary.Count == 2);
            Assert.True(poco.Dictionary.ContainsKey("Key1"));
            Assert.True(poco.Dictionary.ContainsKey("Key2"));
            Assert.Equal(1, poco.Dictionary["Key1"]);
            Assert.Equal(2, poco.Dictionary["Key2"]);
        }
    }
}