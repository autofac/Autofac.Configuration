// This software is part of the Autofac IoC container
// Copyright © 2011 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Autofac.Configuration.Util
{
    /// <summary>
    /// Utilities for converting string configuration values into strongly-typed objects.
    /// </summary>
    internal class TypeManipulation
    {
        /// <summary>
        /// Converts an object to a type compatible with a given parameter.
        /// </summary>
        /// <param name="value">The object value to convert.</param>
        /// <param name="destinationType">The destination <see cref="Type"/> to which <paramref name="value"/> should be converted.</param>
        /// <param name="memberInfo">The parameter for which the <paramref name="value"/> is being converted.</param>
        /// <returns>
        /// An <see cref="Object"/> of type <paramref name="destinationType"/>, converted using
        /// type converters specified on <paramref name="memberInfo"/> if available. If <paramref name="value"/>
        /// is <see langword="null"/> then the output will be <see langword="null"/> for reference
        /// types and the default value for value types.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if conversion of the value fails.
        /// </exception>
        public static object ChangeToCompatibleType(object value, Type destinationType, ParameterInfo memberInfo)
        {
            var attrib = (TypeConverterAttribute)null;
            if (memberInfo != null)
            {
                attrib = memberInfo.GetCustomAttributes(typeof(TypeConverterAttribute), true).Cast<TypeConverterAttribute>().FirstOrDefault();
            }

            return ChangeToCompatibleType(value, destinationType, attrib);
        }

        /// <summary>
        /// Converts an object to a type compatible with a given parameter.
        /// </summary>
        /// <param name="value">The object value to convert.</param>
        /// <param name="destinationType">The destination <see cref="Type"/> to which <paramref name="value"/> should be converted.</param>
        /// <param name="memberInfo">The parameter for which the <paramref name="value"/> is being converted.</param>
        /// <returns>
        /// An <see cref="Object"/> of type <paramref name="destinationType"/>, converted using
        /// type converters specified on <paramref name="memberInfo"/> if available. If <paramref name="value"/>
        /// is <see langword="null"/> then the output will be <see langword="null"/> for reference
        /// types and the default value for value types.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if conversion of the value fails.
        /// </exception>
        public static object ChangeToCompatibleType(object value, Type destinationType, MemberInfo memberInfo)
        {
            var attrib = (TypeConverterAttribute)null;
            if (memberInfo != null)
            {
                attrib = memberInfo.GetCustomAttributes(typeof(TypeConverterAttribute), true).Cast<TypeConverterAttribute>().FirstOrDefault();
            }

            return ChangeToCompatibleType(value, destinationType, attrib);
        }

        /// <summary>
        /// Converts an object to a type compatible with a given parameter.
        /// </summary>
        /// <param name="value">The object value to convert.</param>
        /// <param name="destinationType">The destination <see cref="Type"/> to which <paramref name="value"/> should be converted.</param>
        /// <param name="converterAttribute">A <see cref="TypeConverterAttribute"/>, if available, specifying the type of converter to use.<paramref name="value"/> is being converted.</param>
        /// <returns>
        /// An <see cref="Object"/> of type <paramref name="destinationType"/>, converted using
        /// any type converters specified in <paramref name="converterAttribute"/> if available. If <paramref name="value"/>
        /// is <see langword="null"/> then the output will be <see langword="null"/> for reference
        /// types and the default value for value types.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if conversion of the value fails.
        /// </exception>
        public static object ChangeToCompatibleType(object value, Type destinationType, TypeConverterAttribute converterAttribute = null)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException(nameof(destinationType));
            }

            if (value == null)
            {
                return destinationType.GetTypeInfo().IsValueType ? Activator.CreateInstance(destinationType) : null;
            }

            // Try implicit conversion.
            if (destinationType.IsInstanceOfType(value))
            {
                return value;
            }

            var converter = (TypeConverter)null;

            // Try to get custom type converter information.
            if (converterAttribute != null && !string.IsNullOrEmpty(converterAttribute.ConverterTypeName))
            {
                converter = GetTypeConverterFromName(converterAttribute.ConverterTypeName);
                if (converter.CanConvertFrom(value.GetType()))
                {
                    return converter.ConvertFrom(value);
                }
            }

            // If there's not a custom converter specified via attribute, try for a default.
            converter = TypeDescriptor.GetConverter(value.GetType());
            if (converter.CanConvertTo(destinationType))
            {
                return converter.ConvertTo(value, destinationType);
            }

            // Try explicit opposite conversion.
            converter = TypeDescriptor.GetConverter(destinationType);
            if (converter.CanConvertFrom(value.GetType()))
            {
                return converter.ConvertFrom(value);
            }

            // Try a TryParse method.
            if (value is string)
            {
                var parser = destinationType.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public);
                if (parser != null)
                {
                    var parameters = new[] { value, null };
                    if ((bool)parser.Invoke(null, parameters))
                    {
                        return parameters[1];
                    }
                }
            }

            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ConfigurationResources.TypeConversionUnsupported, value.GetType(), destinationType));
        }

        /// <summary>
        /// Instantiates a type converter from its type name.
        /// </summary>
        /// <param name="converterTypeName">
        /// The name of the <see cref="Type"/> of the <see cref="TypeConverter"/>.
        /// </param>
        /// <returns>
        /// The instantiated <see cref="TypeConverter"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <paramref name="converterTypeName"/> does not correspond
        /// to a <see cref="TypeConverter"/>
        /// </exception>
        private static TypeConverter GetTypeConverterFromName(string converterTypeName)
        {
            var converterType = Type.GetType(converterTypeName, true);
            var converter = Activator.CreateInstance(converterType) as TypeConverter;
            if (converter == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ConfigurationResources.TypeConverterAttributeTypeNotConverter, converterTypeName));
            }

            return converter;
        }
    }
}
