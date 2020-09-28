// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
            Assert.Equal("Val1", poco.List[0]);
            Assert.Equal("Val2", poco.List[1]);
        }

        public class B
        {
            public IList<double> List { get; set; }
        }

        [Fact]
        public void ConvertsTypeInList()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("ConfigurationExtensions_EnumerableParameters.xml").Build();

            var poco = container.Resolve<B>();

            Assert.True(poco.List.Count == 2);
            Assert.Equal(1.234, poco.List[0]);
            Assert.Equal(2.345, poco.List[1]);
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
            Assert.Equal("1.234", poco.List[0]);
            Assert.Equal("2.345", poco.List[1]);
        }

        public class D
        {
            public double Num { get; set; }
        }

        [Fact]
        public void InjectsSingleValueWithConversion()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("ConfigurationExtensions_EnumerableParameters.xml").Build();

            var poco = container.Resolve<D>();

            Assert.Equal(123.456, poco.Num);
        }

        public class E
        {
            public IList<double> List { get; set; }

            public E(IList<double> list)
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
            Assert.Equal(1.234, poco.List[0]);
            Assert.Equal(2.345, poco.List[1]);
        }

        [Theory]
        [MemberData(nameof(ParsingCultures))]
        public void TypeConversionsAreCaseInvariant(CultureInfo culture)
        {
            // Issue #26 - parsing needs to be InvariantCulture or config fails
            // when it's moved from machine to machine.
            TestCulture.With(
                culture,
                () =>
                {
                    var container = EmbeddedConfiguration.ConfigureContainerWithXml("ConfigurationExtensions_EnumerableParameters.xml").Build();

                    var poco = container.Resolve<E>();

                    Assert.True(poco.List.Count == 2);
                    Assert.Equal(1.234, poco.List[0]);
                    Assert.Equal(2.345, poco.List[1]);
                });
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
            Assert.Equal("Val1", enumerable[0]);
            Assert.Equal("Val2", enumerable[1]);
        }

        public class H
        {
            public IEnumerable<double> Enumerable { get; set; }
        }

        [Fact]
        public void InjectsGenericIEnumerable()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("ConfigurationExtensions_EnumerableParameters.xml").Build();

            var poco = container.Resolve<H>();

            Assert.NotNull(poco.Enumerable);
            var enumerable = poco.Enumerable.ToList();
            Assert.True(enumerable.Count == 2);
            Assert.Equal(1.234, enumerable[0]);
            Assert.Equal(2.345, enumerable[1]);
        }

        public class I
        {
            public ICollection<double> Collection { get; set; }
        }

        [Fact]
        public void InjectsGenericCollection()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("ConfigurationExtensions_EnumerableParameters.xml").Build();

            var poco = container.Resolve<I>();

            Assert.NotNull(poco.Collection);
            Assert.True(poco.Collection.Count == 2);
            Assert.Equal(1.234, poco.Collection.First());
            Assert.Equal(2.345, poco.Collection.Last());
        }

        public class J
        {
            public J(IList<string> list)
            {
                List = list;
            }

            public IList<string> List { get; private set; }
        }

        [Fact]
        public void ParameterStringListInjection()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("ConfigurationExtensions_EnumerableParameters.xml").Build();

            var poco = container.Resolve<J>();

            Assert.True(poco.List.Count == 2);
            Assert.Equal("Val1", poco.List[0]);
            Assert.Equal("Val2", poco.List[1]);
        }

        public class K
        {
            public K(IList<string> list = null)
            {
                List = list;
            }

            public IList<string> List { get; private set; }
        }

        [Fact]
        public void ParameterStringListInjectionOptionalParameter()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("ConfigurationExtensions_EnumerableParameters.xml").Build();

            var poco = container.Resolve<K>();

            Assert.True(poco.List.Count == 2);
            Assert.Equal("Val1", poco.List[0]);
            Assert.Equal("Val2", poco.List[1]);
        }

        public class L
        {
            public L()
            {
                List = new List<string>();
            }

            public L(IList<string> list = null)
            {
                List = list;
            }

            public IList<string> List { get; private set; }
        }

        [Fact]
        public void ParameterStringListInjectionMultipleConstructors()
        {
            var container = EmbeddedConfiguration.ConfigureContainerWithXml("ConfigurationExtensions_EnumerableParameters.xml").Build();

            var poco = container.Resolve<L>();

            Assert.True(poco.List.Count == 2);
            Assert.Equal("Val1", poco.List[0]);
            Assert.Equal("Val2", poco.List[1]);
        }

        public class M
        {
            public M(IList<string> list) => List = list;

            public IList<string> List { get; }
        }

        /// <summary>
        /// A characterization test, not intended to express desired behaviour, but to capture the current behaviour.
        /// </summary>
        [Fact]
        public void ParameterStringListInjectionSecondElementHasNoName()
        {
            var container = EmbeddedConfiguration
                .ConfigureContainerWithXml("ConfigurationExtensions_EnumerableParameters.xml").Build();

            var poco = container.Resolve<M>();

            // Val2 is dropped from the configuration when it's parsed.
            Assert.Collection(poco.List, v => Assert.Equal("Val1", v));
        }

        public static IEnumerable<object[]> ParsingCultures()
        {
            yield return new object[] { new CultureInfo("en-US") };
            yield return new object[] { new CultureInfo("es-MX") };
            yield return new object[] { new CultureInfo("it-IT") };
            yield return new object[] { CultureInfo.InvariantCulture };
        }
    }
}
