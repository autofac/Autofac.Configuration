using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Xml;

namespace Autofac.Configuration.Test
{
    public static class EmbeddedConfiguration
    {
        public static ContainerBuilder ConfigureContainer(IConfiguration configuration)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new ConfigurationModule(configuration));
            return builder;
        }

        public static ContainerBuilder ConfigureContainerWithJson(string configFile)
        {
            return ConfigureContainer(LoadJson(configFile));
        }

        public static ContainerBuilder ConfigureContainerWithXml(string configFile)
        {
            return ConfigureContainer(LoadXml(configFile));
        }

        public static IConfiguration LoadJson(string configFile)
        {
            using (var stream = GetEmbeddedFileStream(configFile))
            {
                var provider = new EmbeddedConfigurationProvider<JsonConfigurationSource>(stream);
                var config = new ConfigurationRoot(new List<IConfigurationProvider> { provider });
                return config;
            }
        }

        public static IConfiguration LoadXml(string configFile)
        {
            using (var stream = GetEmbeddedFileStream(configFile))
            {
                var provider = new EmbeddedConfigurationProvider<XmlConfigurationSource>(stream);
                var config = new ConfigurationRoot(new List<IConfigurationProvider> { provider });
                return config;
            }
        }

        private static Stream GetEmbeddedFileStream(string configFile)
        {
            return typeof(EmbeddedConfiguration).GetTypeInfo().Assembly.GetManifestResourceStream("Autofac.Configuration.Test.Files." + configFile);
        }
    }
}
