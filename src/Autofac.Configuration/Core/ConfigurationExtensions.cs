using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Autofac.Configuration.Util;
using Autofac.Core;
using Microsoft.Framework.ConfigurationModel;

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
                throw new ArgumentNullException("configuration");
            }

            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (String.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, ConfigurationResources.ArgumentMayNotBeEmpty, "configuration key"), "key");
            }

            var assemblyName = configuration.Get(key);
            if (String.IsNullOrWhiteSpace(assemblyName))
            {
                return null;
            }

            return Assembly.Load(new AssemblyName(assemblyName));
        }

        public static IEnumerable<Parameter> GetParameters(this IConfiguration configuration, string key)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (String.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, ConfigurationResources.ArgumentMayNotBeEmpty, "configuration key"), "key");
            }

            foreach (var parameterElement in configuration.GetSubKeys(key))
            {
                var parameterValue = GetConfiguredParameterValue(parameterElement.Value);
                var parameterName = parameterElement.Key;
                yield return new ResolvedParameter(
                    (pi, c) => String.Equals(pi.Name, parameterName, StringComparison.OrdinalIgnoreCase),
                    (pi, c) => TypeManipulation.ChangeToCompatibleType(parameterValue, pi.ParameterType, pi));
            }
        }

        public static IEnumerable<Parameter> GetProperties(this IConfiguration configuration, string key)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (String.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, ConfigurationResources.ArgumentMayNotBeEmpty, "configuration key"), "key");
            }

            foreach (var propertyElement in configuration.GetSubKeys(key))
            {
                var parameterValue = GetConfiguredParameterValue(propertyElement.Value);
                var parameterName = propertyElement.Key;
                yield return new ResolvedParameter(
                    (pi, c) =>
                {
                    PropertyInfo prop;
                    return pi.TryGetDeclaringProperty(out prop) &&
                        String.Equals(prop.Name, parameterName, StringComparison.OrdinalIgnoreCase);
                },
                    (pi, c) =>
                {
                    var prop = (PropertyInfo)null;
                    pi.TryGetDeclaringProperty(out prop);
                    return TypeManipulation.ChangeToCompatibleType(parameterValue, pi.ParameterType, prop);
                });
            }
        }

        private static object GetConfiguredParameterValue(IConfiguration value)
        {
            var subKeys = value.GetSubKeys();
            if(!subKeys.Any())
            {
                return value.Get(null);
            }

            int parsed;
            if(subKeys.All(sk => Int32.TryParse(sk.Key, out parsed)))
            {
                // All the subkeys are integers - it's a list. Note there's a gotcha
                // here where you can't really express a Dictionary<int, T> via configuration
                // due to the way lists are handled in ConfigurationModel.
                var list = new List<string>();
                foreach(var subKey in subKeys)
                {
                    list.Add(subKey.Value.Get(null));
                }

                return new ConfiguredListParameter { List = list.ToArray() };
            }

            // TODO: There are subkeys but not all numbers - it's a dictionary.
            return null;
        }
    }
}