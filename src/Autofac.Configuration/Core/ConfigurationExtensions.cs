using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Autofac.Configuration.Util;
using Autofac.Core;
using Microsoft.Extensions.Configuration;

namespace Autofac.Configuration.Core
{
    /// <summary>
    /// Extension methods for working with <see cref="IConfiguration"/>.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Reads the default assembly information from configuration and
        /// parses the assembly name into an assembly.
        /// </summary>
        /// <param name="configuration">
        /// An <see cref="IConfiguration"/> from which the default assembly
        /// should be read.
        /// </param>
        /// <returns>
        /// An <see cref="Assembly"/> if the default assembly is specified on
        /// <paramref name="configuration"/>; or <see langword="null"/> if not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="configuration"/> is <see langword="null"/>.
        /// </exception>
        public static Assembly DefaultAssembly(this IConfiguration configuration)
        {
            return configuration.GetAssembly("defaultAssembly");
        }

        /// <summary>
        /// Reads an assembly name from configuration and parses the assembly name into an assembly.
        /// </summary>
        /// <param name="configuration">
        /// An <see cref="IConfiguration"/> from which the default assembly
        /// should be read.
        /// </param>
        /// <param name="key">
        /// The <see cref="String"/> key in configuration where the assembly name
        /// is specified.
        /// </param>
        /// <returns>
        /// An <see cref="Assembly"/> if the assembly is specified on
        /// <paramref name="configuration"/>; or <see langword="null"/> if not.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="configuration"/> or <paramref name="key"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="key"/> is empty or whitespace.
        /// </exception>
        public static Assembly GetAssembly(this IConfiguration configuration, string key)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ConfigurationResources.ArgumentMayNotBeEmpty, "configuration key"), nameof(key));
            }

            string assemblyName = configuration[key];
            if (string.IsNullOrWhiteSpace(assemblyName))
            {
                return null;
            }

            return Assembly.Load(new AssemblyName(assemblyName));
        }

        /// <summary>
        /// Converts configured parameter values into parameters that can be used
        /// during object resolution.
        /// </summary>
        /// <param name="configuration">
        /// The <see cref="IConfiguration"/> element that contains the component
        /// with defined parameters.
        /// </param>
        /// <param name="key">
        /// The <see cref="String"/> key indicating the sub-element with the
        /// parameters. Usually this is <c>parameters</c>.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="Parameter"/> values
        /// that can be used during object resolution.
        /// </returns>
        public static IEnumerable<Parameter> GetParameters(this IConfiguration configuration, string key)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ConfigurationResources.ArgumentMayNotBeEmpty, "configuration key"), nameof(key));
            }

            foreach (var parameterElement in configuration.GetSection(key).GetChildren())
            {
                var parameterValue = GetConfiguredParameterValue(parameterElement);
                string parameterName = GetKeyName(parameterElement.Key);
                yield return new ResolvedParameter(
                    (pi, c) => string.Equals(pi.Name, parameterName, StringComparison.OrdinalIgnoreCase),
                    (pi, c) => TypeManipulation.ChangeToCompatibleType(parameterValue, pi.ParameterType, pi));
            }
        }

        /// <summary>
        /// Converts configured property values into parameters that can be used
        /// during object resolution.
        /// </summary>
        /// <param name="configuration">
        /// The <see cref="IConfiguration"/> element that contains the component
        /// with defined properties.
        /// </param>
        /// <param name="key">
        /// The <see cref="String"/> key indicating the sub-element with the
        /// propeties. Usually this is <c>properties</c>.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="Parameter"/> values
        /// that can be used during object resolution.
        /// </returns>
        public static IEnumerable<Parameter> GetProperties(this IConfiguration configuration, string key)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ConfigurationResources.ArgumentMayNotBeEmpty, "configuration key"), nameof(key));
            }

            foreach (var propertyElement in configuration.GetSection(key).GetChildren())
            {
                var parameterValue = GetConfiguredParameterValue(propertyElement);
                string parameterName = GetKeyName(propertyElement.Key);
                yield return new ResolvedParameter(
                    (pi, c) =>
                {
                    PropertyInfo prop;
                    return pi.TryGetDeclaringProperty(out prop) &&
string.Equals(prop.Name, parameterName, StringComparison.OrdinalIgnoreCase);
                },
                    (pi, c) =>
                {
                    var prop = (PropertyInfo)null;
                    pi.TryGetDeclaringProperty(out prop);
                    return TypeManipulation.ChangeToCompatibleType(parameterValue, pi.ParameterType, prop);
                });
            }
        }

        /// <summary>
        /// Loads a type by name.
        /// </summary>
        /// <param name="configuration">
        /// The <see cref="IConfiguration"/> object containing the type value to load.
        /// </param>
        /// <param name="key">
        /// Name of the <see cref="System.Type"/> to load. This may be a partial type name or a fully-qualified type name.
        /// </param>
        /// <param name="defaultAssembly">
        /// The default <see cref="System.Reflection.Assembly"/> to use in type resolution if <paramref name="key"/>
        /// is a partial type name.
        /// </param>
        /// <returns>
        /// The resolved <see cref="System.Type"/> based on the specified name.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="configuration"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the specified <paramref name="key"/> can't be resolved as a fully-qualified type name and
        /// isn't a partial type name for a <see cref="System.Type"/> found in the <paramref name="defaultAssembly"/>.
        /// </exception>
        public static Type GetType(this IConfiguration configuration, string key, Assembly defaultAssembly)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            string typeName = configuration[key];
            var type = Type.GetType(typeName);

            if (type == null && defaultAssembly != null)
            {
                // Don't throw on error; we'll check it later.
                type = defaultAssembly.GetType(typeName, false, true);
            }

            if (type == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ConfigurationResources.TypeNotFound, typeName));
            }

            return type;
        }

        /// <summary>
        /// Inspects a parameter/property value to determine if it's a scalar,
        /// list, or dictionary property and casts it appropriately.
        /// </summary>
        /// <param name="value">
        /// The <see cref="IConfigurationSection"/> object containing the parameter/property
        /// value to parse.
        /// </param>
        /// <returns>
        /// A value that can be type-converted and used during object resolution.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The Microsoft configuration model code sees arrays (lists) the same
        /// as a dictionary with numeric keys. We have to do some work to determine
        /// how to store the parsed configuration values so they can be converted
        /// appropriately at resolve time; and there's still an edge case where
        /// someone actually wanted a <see cref="Dictionary{TKey, TValue}"/> with
        /// sequential, zero-based numeric keys.
        /// </para>
        /// </remarks>
        private static object GetConfiguredParameterValue(IConfigurationSection value)
        {
            var subKeys = value.GetChildren().Select(sk => new Tuple<string, string>(GetKeyName(sk.Key), sk.Value)).ToArray();
            if (subKeys.Length == 0)
            {
                // No subkeys indicates a scalar value.
                return value.Value;
            }

            int parsed;
            if (subKeys.All(sk => int.TryParse(sk.Item1, out parsed)))
            {
                int i = 0;
                bool isList = true;
                foreach (int subKey in subKeys.Select(sk => int.Parse(sk.Item1, CultureInfo.InvariantCulture)))
                {
                    if (subKey != i)
                    {
                        isList = false;
                        break;
                    }

                    i++;
                }

                if (isList)
                {
                    var list = new List<string>();
                    foreach (var subKey in subKeys)
                    {
                        list.Add(subKey.Item2);
                    }

                    return new ConfiguredListParameter { List = list.ToArray() };
                }
            }

            // There are subkeys but not all zero-based sequential numbers - it's a dictionary.
            var dict = new Dictionary<string, string>();
            foreach (var subKey in subKeys)
            {
                dict[subKey.Item1] = subKey.Item2;
            }

            return new ConfiguredDictionaryParameter { Dictionary = dict };
        }

        /// <summary>
        /// Gets the simple configuration key name from a full, colon-delimited
        /// configuration key name.
        /// </summary>
        /// <param name="fullKey">The full configuration key name, like <c>configuration:full:key</c>.</param>
        /// <returns>
        /// The last segment in the colon-delimited full key name, like <c>key</c>.
        /// </returns>
        private static string GetKeyName(string fullKey)
        {
            int index = fullKey.LastIndexOf(':');
            if (index < 0)
            {
                return fullKey;
            }

            return fullKey.Substring(index + 1);
        }
    }
}