using System.Collections;
using System.Collections.Generic;
using Autofac.Configuration;
using Xunit;

namespace Autofac.Tests.Configuration
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
            var container = ConfigureContainer("DictionaryParameters").Build();

            var poco = container.Resolve<A>();

            Assert.True(poco.Dictionary.Count == 2);
            Assert.True(poco.Dictionary.ContainsKey("1"));
            Assert.True(poco.Dictionary.ContainsKey("2"));
            Assert.Equal("Val1", poco.Dictionary["1"]);
            Assert.Equal("Val2", poco.Dictionary["2"]);
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
            var container = ConfigureContainer("DictionaryParameters").Build();

            var poco = container.Resolve<B>();

            Assert.True(poco.Dictionary.Count == 2);
            Assert.True(poco.Dictionary.ContainsKey("1"));
            Assert.True(poco.Dictionary.ContainsKey("2"));
            Assert.Equal("Val1", poco.Dictionary["1"]);
            Assert.Equal("Val2", poco.Dictionary["2"]);
        }

        public class C
        {
            public IDictionary Dictionary { get; set; }
        }

        [Fact]
        public void InjectsNonGenericDictionary()
        {
            var container = ConfigureContainer("DictionaryParameters").Build();

            var poco = container.Resolve<C>();

            Assert.True(poco.Dictionary.Count == 2);
            Assert.True(poco.Dictionary.Contains("1"));
            Assert.True(poco.Dictionary.Contains("2"));
            Assert.Equal("Val1", poco.Dictionary["1"]);
            Assert.Equal("Val2", poco.Dictionary["2"]);
        }

        public class D
        {
            public Dictionary<string, string> Dictionary { get; set; }
        }

        [Fact]
        public void InjectsConcreteDictionary()
        {
            var container = ConfigureContainer("DictionaryParameters").Build();

            var poco = container.Resolve<D>();

            Assert.True(poco.Dictionary.Count == 2);
            Assert.True(poco.Dictionary.ContainsKey("1"));
            Assert.True(poco.Dictionary.ContainsKey("2"));
            Assert.Equal("Val1", poco.Dictionary["1"]);
            Assert.Equal("Val2", poco.Dictionary["2"]);
        }

        public class E
        {
            public IDictionary<int, string> Dictionary { get; set; }
        }

        [Fact]
        public void ConvertsDictionaryKey()
        {
            var container = ConfigureContainer("DictionaryParameters").Build();

            var poco = container.Resolve<E>();

            Assert.True(poco.Dictionary.Count == 2);
            Assert.True(poco.Dictionary.ContainsKey(1));
            Assert.True(poco.Dictionary.ContainsKey(2));
            Assert.Equal("Val1", poco.Dictionary[1]);
            Assert.Equal("Val2", poco.Dictionary[2]);
        }

        public class F
        {
            public IDictionary<string, int> Dictionary { get; set; }
        }

        [Fact]
        public void ConvertsDictionaryValue()
        {
            var container = ConfigureContainer("DictionaryParameters").Build();

            var poco = container.Resolve<F>();

            Assert.True(poco.Dictionary.Count == 2);
            Assert.True(poco.Dictionary.ContainsKey("1"));
            Assert.True(poco.Dictionary.ContainsKey("2"));
            Assert.Equal(1, poco.Dictionary["1"]);
            Assert.Equal(2, poco.Dictionary["2"]);
        }

        public class G
        {
            public IDictionary<string, string> Dictionary { get; set; }
        }

        [Fact]
        public void InjectsEmptyDictionary()
        {
            var container = ConfigureContainer("DictionaryParameters").Build();

            var poco = container.Resolve<G>();

            Assert.NotNull(poco.Dictionary);
            Assert.True(poco.Dictionary.Count == 0);
        }

        static ContainerBuilder ConfigureContainer(string configFile)
        {
            var cb = new ContainerBuilder();
            var fullFilename = "Files/" + configFile + ".config";
            var csr = new ConfigurationSettingsReader(SectionHandler.DefaultSectionName, fullFilename);
            cb.RegisterModule(csr);
            return cb;
        }
    }
}