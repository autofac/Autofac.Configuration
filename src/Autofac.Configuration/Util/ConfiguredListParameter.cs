using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Autofac.Configuration.Util
{
    [TypeConverter(typeof(ListTypeConverter))]
    internal class ConfiguredListParameter
    {
        public string[] List { get; set; }

        private class ListTypeConverter : TypeConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                var instantiatableType = GetInstantiableType(destinationType);

                var castValue = value as ConfiguredListParameter;
                if (castValue != null && instantiatableType != null)
                {
                    Type[] generics = instantiatableType.GetGenericArguments();
                    var collection = (IList)Activator.CreateInstance(instantiatableType);
                    foreach (var item in castValue.List)
                    {
                        collection.Add(TypeManipulation.ChangeToCompatibleType(item, generics[0]));
                    }

                    return collection;
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (GetInstantiableType(destinationType) != null)
                {
                    return true;
                }

                return base.CanConvertTo(context, destinationType);
            }

            private static Type GetInstantiableType(Type destinationType)
            {
                if (typeof(IEnumerable).IsAssignableFrom(destinationType))
                {
                    var generics = destinationType.IsConstructedGenericType ? destinationType.GetGenericArguments() : new[] { typeof(object) };
                    if (generics.Length != 1)
                    {
                        return null;
                    }

                    var listType = typeof(List<>).MakeGenericType(generics);

                    if (destinationType.IsAssignableFrom(listType))
                    {
                        return listType;
                    }
                }

                return null;
            }
        }
    }
}
