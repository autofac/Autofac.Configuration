using Microsoft.Framework.ConfigurationModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Autofac.Configuration.Test;
using Autofac.Configuration.Util;

namespace Autofac.Configuration.Test.Core
{
    public class TypeConverterParametersFixture
    {
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

        private static IContainer ConfigureContainer()
        {
            return EmbeddedConfiguration.ConfigureContainerWithJson("TypeConverterParameters.json").Build();
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
                return sourceType == typeof(ConfiguredListParameter) || base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value == null)
                {
                    return null;
                }
                var castValue = value as ConfiguredListParameter;
                if (castValue == null)
                {
                    return base.ConvertFrom(context, culture, value);
                }
                var list = new List<Convertible>();
                var converter = new ConvertibleConverter();
                foreach (var item in castValue.List)
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
                return sourceType == typeof(ConfiguredDictionaryParameter) || base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value == null)
                {
                    return null;
                }
                var castValue = value as ConfiguredDictionaryParameter;
                if (castValue == null)
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
    }
}
