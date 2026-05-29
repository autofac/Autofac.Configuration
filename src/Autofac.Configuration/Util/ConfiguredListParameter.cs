// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.ComponentModel;

namespace Autofac.Configuration.Util;

/// <summary>
/// Configuration settings that provide a list parameter to a registration.
/// </summary>
[TypeConverter(typeof(ListTypeConverter))]
internal class ConfiguredListParameter
{
    /// <summary>
    /// Gets or sets the list of raw values.
    /// </summary>
    public string[]? List
    {
        get; set;
    }

    private sealed class ListTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        {
            return GetInstantiableListType(destinationType) != null ||
                    GetInstantiableDictionaryType(destinationType) != null ||
                    base.CanConvertTo(context, destinationType);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is not ConfiguredListParameter castValue)
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }

            return ConvertConfiguredList(castValue, destinationType) ?? base.ConvertTo(context, culture, value, destinationType);
        }

        private static object? ConvertConfiguredList(ConfiguredListParameter configuredList, Type destinationType)
        {
            return ConvertToInstantiableList(configuredList, destinationType) ?? ConvertToInstantiableDictionary(configuredList, destinationType);
        }

        private static object? ConvertToInstantiableList(ConfiguredListParameter configuredList, Type destinationType)
        {
            // 99% of the time this type of parameter will be associated
            // with an ordinal list - List<T> or T[] sort of thing.
            var instantiableType = GetInstantiableListType(destinationType);
            if (instantiableType == null)
            {
                return null;
            }

            var collection = (IList)Activator.CreateInstance(instantiableType)!;
            if (configuredList.List == null)
            {
                return collection;
            }

            var elementType = instantiableType.GetGenericArguments()[0];
            foreach (var item in configuredList.List)
            {
                collection.Add(TypeManipulation.ChangeToCompatibleType(item, elementType));
            }

            return collection;
        }

        private static object? ConvertToInstantiableDictionary(ConfiguredListParameter configuredList, Type destinationType)
        {
            // Rare edge case for Dictionary<int, T>-style ordinal keys.
            var instantiableType = GetInstantiableDictionaryType(destinationType);
            if (instantiableType == null)
            {
                return null;
            }

            var dictionary = (IDictionary)Activator.CreateInstance(instantiableType)!;
            if (configuredList.List == null)
            {
                return dictionary;
            }

            var generics = instantiableType.GetGenericArguments();
            for (var i = 0; i < configuredList.List.Length; i++)
            {
                var convertedKey = TypeManipulation.ChangeToCompatibleType(i, generics[0])!;
                var convertedValue = TypeManipulation.ChangeToCompatibleType(configuredList.List[i], generics[1]);
                dictionary.Add(convertedKey, convertedValue);
            }

            return dictionary;
        }

        /// <summary>
        /// Handles type determination for the case where the dictionary
        /// has numeric/ordinal keys.
        /// </summary>
        /// <param name="destinationType">
        /// The type to which the list content should be converted.
        /// </param>
        /// <returns>
        /// A dictionary type where the key can be numeric.
        /// </returns>
        private static Type? GetInstantiableDictionaryType(Type? destinationType)
        {
            if (destinationType is not null &&
                (typeof(IDictionary).IsAssignableFrom(destinationType) ||
                (destinationType.IsConstructedGenericType && typeof(IDictionary<,>).IsAssignableFrom(destinationType.GetGenericTypeDefinition()))))
            {
                var generics = destinationType.IsConstructedGenericType ? destinationType.GetGenericArguments() : new[] { typeof(int), typeof(object) };
                if (generics.Length != 2)
                {
                    return null;
                }

                var dictType = typeof(Dictionary<,>).MakeGenericType(generics);
                if (destinationType.IsAssignableFrom(dictType))
                {
                    return dictType;
                }
            }

            return null;
        }

        /// <summary>
        /// Handles type determination list conversion.
        /// </summary>
        /// <param name="destinationType">
        /// The type to which the list content should be converted.
        /// </param>
        /// <returns>
        /// A list type compatible with the data values.
        /// </returns>
        private static Type? GetInstantiableListType(Type? destinationType)
        {
            if (destinationType is not null && typeof(IEnumerable).IsAssignableFrom(destinationType))
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
