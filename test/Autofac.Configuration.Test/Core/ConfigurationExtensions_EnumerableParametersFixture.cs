using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Autofac.Configuration.Test.Core
{
    public class ConfigurationExtensions_EnumerableParametersFixture
    {
        public class A
        {
            public IList<string> List { get; set; }
        }

        [Fact]
        public void PropertyStringListInjection()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("ConfigurationExtensions_EnumerableParameters.xml").Build();

            var poco = container.Resolve<A>();

            Assert.True(poco.List.Count == 2);
            Assert.Equal(poco.List[0], "Val1");
            Assert.Equal(poco.List[1], "Val2");
        }

        public class B
        {
            public IList<int> List { get; set; }
        }

        [Fact]
        public void ConvertsTypeInList()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("ConfigurationExtensions_EnumerableParameters.xml").Build();

            var poco = container.Resolve<B>();

            Assert.True(poco.List.Count == 2);
            Assert.Equal(poco.List[0], 1);
            Assert.Equal(poco.List[1], 2);
        }

        public class C
        {
            public IList List { get; set; }
        }

        [Fact]
        public void FillsNonGenericListWithString()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("ConfigurationExtensions_EnumerableParameters.xml").Build();

            var poco = container.Resolve<C>();

            Assert.True(poco.List.Count == 2);
            Assert.Equal(poco.List[0], "1");
            Assert.Equal(poco.List[1], "2");
        }

        public class D
        {
            public int Num { get; set; }
        }

        [Fact]
        public void InjectsSingleValueWithConversion()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("ConfigurationExtensions_EnumerableParameters.xml").Build();

            var poco = container.Resolve<D>();

            Assert.True(poco.Num == 123);
        }

        public class E
        {
            public IList<int> List { get; set; }

            public E(IList<int> list)
            {
                List = list;
            }
        }

        [Fact]
        public void InjectsConstructorParameter()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("ConfigurationExtensions_EnumerableParameters.xml").Build();

            var poco = container.Resolve<E>();

            Assert.True(poco.List.Count == 2);
            Assert.Equal(poco.List[0], 1);
            Assert.Equal(poco.List[1], 2);
        }

        public class G
        {
            public IEnumerable Enumerable { get; set; }
        }

        [Fact]
        public void InjectsIEnumerable()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("ConfigurationExtensions_EnumerableParameters.xml").Build();

            var poco = container.Resolve<G>();

            Assert.NotNull(poco.Enumerable);
            var enumerable = poco.Enumerable.Cast<string>().ToList();
            Assert.True(enumerable.Count == 2);
            Assert.Equal(enumerable[0], "Val1");
            Assert.Equal(enumerable[1], "Val2");
        }

        public class H
        {
            public IEnumerable<int> Enumerable { get; set; }
        }

        [Fact]
        public void InjectsGenericIEnumerable()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("ConfigurationExtensions_EnumerableParameters.xml").Build();

            var poco = container.Resolve<H>();

            Assert.NotNull(poco.Enumerable);
            var enumerable = poco.Enumerable.ToList();
            Assert.True(enumerable.Count == 2);
            Assert.Equal(enumerable[0], 1);
            Assert.Equal(enumerable[1], 2);
        }

        public class I
        {
            public ICollection<int> Collection { get; set; }
        }

        [Fact]
        public void InjectsGenericCollection()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("ConfigurationExtensions_EnumerableParameters.xml").Build();

            var poco = container.Resolve<I>();

            Assert.NotNull(poco.Collection);
            Assert.True(poco.Collection.Count == 2);
            Assert.Equal(poco.Collection.First(), 1);
            Assert.Equal(poco.Collection.Last(), 2);
        }
    }
}
