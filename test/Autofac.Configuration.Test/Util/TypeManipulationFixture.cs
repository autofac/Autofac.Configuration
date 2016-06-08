using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using Autofac.Configuration.Util;
using Xunit;

namespace Autofac.Configuration.Test.Util
{
    public class TypeManipulationFixture
    {
        [Fact]
        public void ChangeToCompatibleType_LooksForTryParseMethod()
        {
            var address = "127.0.0.1";
            var value = TypeManipulation.ChangeToCompatibleType(address, typeof(IPAddress));
            Assert.Equal(value, IPAddress.Parse(address));
        }

        [Fact]
        public void ChangeToCompatibleType_UsesTypeConverterOnParameter()
        {
            var ctor = typeof(HasTypeConverterAttributes).GetConstructor(new Type[] { typeof(Convertible) });
            var member = ctor.GetParameters().First();
            var actual = TypeManipulation.ChangeToCompatibleType("25", typeof(Convertible), member) as Convertible;
            Assert.NotNull(actual);
            Assert.Equal(25, actual.Value);
        }

        [Fact]
        public void ChangeToCompatibleType_UsesTypeConverterOnProperty()
        {
            var member = typeof(HasTypeConverterAttributes).GetProperty("Property");
            var actual = TypeManipulation.ChangeToCompatibleType("25", typeof(Convertible), member) as Convertible;
            Assert.NotNull(actual);
            Assert.Equal(25, actual.Value);
        }

        [Fact]
        public void ChangeToCompatibleType_NullReferenceType()
        {
            var actual = TypeManipulation.ChangeToCompatibleType(null, typeof(String));
            Assert.Null(actual);
        }

        [Fact]
        public void ChangeToCompatibleType_NullValueType()
        {
            var actual = TypeManipulation.ChangeToCompatibleType(null, typeof(Int32));
            Assert.Equal(0, actual);
        }

        [Fact]
        public void ChangeToCompatibleType_NoConversionNeeded()
        {
            var actual = TypeManipulation.ChangeToCompatibleType(15, typeof(Int32));
            Assert.Equal(15, actual);
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

        public class HasTypeConverterAttributes
        {
            public HasTypeConverterAttributes([TypeConverter(typeof(ConvertibleConverter))] Convertible parameter)
            {
                this.Property = parameter;
            }

            [TypeConverter(typeof(ConvertibleConverter))]
            public Convertible Property { get; set; }
        }
    }
}