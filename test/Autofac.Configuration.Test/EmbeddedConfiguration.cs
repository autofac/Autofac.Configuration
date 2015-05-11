using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Autofac.Configuration.Core;
using Autofac.Core;
using Microsoft.Framework.ConfigurationModel;
using Xunit;
using ConfigModel = Microsoft.Framework.ConfigurationModel.Configuration;

namespace Autofac.Configuration.Test
{
    public static class EmbeddedConfiguration
    {
        public static IConfiguration LoadJson(string configFile)
        {
            var config = new ConfigModel();
            var source = new JsonConfigurationSource("path", true);
            using (var stream = typeof(EmbeddedConfiguration).GetTypeInfo().Assembly.GetManifestResourceStream("Files/" + configFile))
            {
                typeof(JsonConfigurationSource).GetMethod("Load", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(source, new object[] { stream });
            }
            config.Add(source, false);
            return config;
        }

        public static IConfiguration LoadXml(string configFile)
        {
            var config = new ConfigModel();
            var source = new XmlConfigurationSource("path");
            using (var stream = typeof(EmbeddedConfiguration).GetTypeInfo().Assembly.GetManifestResourceStream("Files/" + configFile))
            {
                typeof(XmlConfigurationSource).GetMethod("Load", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(source, new object[] { stream });
            }
            config.Add(source, false);
            return config;
        }

        public static ContainerBuilder ConfigureContainer(IConfiguration configuration)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new ConfigurationModule(configuration));
            return builder;
        }

        public static ContainerBuilder ConfigureContainerWithXml(string configFile)
        {
            return ConfigureContainer(LoadXml(configFile));
        }

        public static ContainerBuilder ConfigureContainerWithJson(string configFile)
        {
            return ConfigureContainer(LoadJson(configFile));
        }
    }
}
