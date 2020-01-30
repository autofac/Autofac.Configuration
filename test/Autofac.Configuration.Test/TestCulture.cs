using System;
using System.Globalization;
using System.Threading;
using Xunit;

namespace Autofac.Configuration.Test
{
    public static class TestCulture
    {
        public static void With(CultureInfo culture, Action test)
        {
            var originalCulture = Thread.CurrentThread.CurrentCulture;
            var originalUICulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            CultureInfo.CurrentCulture.ClearCachedData();
            CultureInfo.CurrentUICulture.ClearCachedData();
            try
            {
                test();
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
                Thread.CurrentThread.CurrentUICulture = originalUICulture;
                CultureInfo.CurrentCulture.ClearCachedData();
                CultureInfo.CurrentUICulture.ClearCachedData();
            }
        }
    }
}