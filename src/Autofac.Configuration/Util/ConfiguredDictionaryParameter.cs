﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Autofac.Configuration.Util
{
    /// <summary>
    /// Configuration settings that provide a dictionary parameter to a registration.
    /// </summary>
    [TypeConverter(typeof(DictionaryTypeConverter))]
    internal class ConfiguredDictionaryParameter
    {
        /// <summary>
        /// Gets or sets the dictionary of raw values.
        /// </summary>
        public Dictionary<string, string> Dictionary { get; set; }

        private class DictionaryTypeConverter : TypeConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                var instantiatableType = GetInstantiableType(destinationType);

                if (value is ConfiguredDictionaryParameter castValue && instantiatableType != null)
                {
                    var dictionary = (IDictionary)Activator.CreateInstance(instantiatableType);
                    var generics = instantiatableType.GetGenericArguments();

                    foreach (var item in castValue.Dictionary)
                    {
                        if (string.IsNullOrEmpty(item.Key))
                        {
                            throw new FormatException(ConfigurationResources.DictionaryKeyMayNotBeNullOrEmpty);
                        }

                        var convertedKey = TypeManipulation.ChangeToCompatibleType(item.Key, generics[0]);
                        var convertedValue = TypeManipulation.ChangeToCompatibleType(item.Value, generics[1]);

                        dictionary.Add(convertedKey, convertedValue);
                    }

                    return dictionary;
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
                if (typeof(IDictionary).IsAssignableFrom(destinationType) ||
                    (destinationType.IsConstructedGenericType && typeof(IDictionary<,>).IsAssignableFrom(destinationType.GetGenericTypeDefinition())))
                {
                    var generics = destinationType.IsConstructedGenericType ? destinationType.GetGenericArguments() : new[] { typeof(string), typeof(object) };
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
        }
    }
}
