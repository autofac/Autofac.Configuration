using System;
using System.Globalization;

namespace Autofac.Configuration.Util
{
    /// <summary>
    /// Extension methods for parsing string values from configuration.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Uses a flexible parsing routine to convert a text value into
        /// a <see cref="Boolean"/>.
        /// </summary>
        /// <param name="value">
        /// The value to parse into a <see cref="Boolean"/>.
        /// </param>
        /// <returns>
        /// <see langword="true" /> or <see langword="false" /> based on the
        /// content of the <paramref name="value" />.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <paramref name="value" /> can't be parsed into a <see cref="Boolean"/>.
        /// </exception>
        public static bool ToFlexibleBoolean(this string value)
        {
            if (string.IsNullOrWhiteSpace(value) ||
                value.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("no", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("n", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("0", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            else if (value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("y", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("1", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ConfigurationResources.UnrecognisedBoolean, value));
        }
    }
}
