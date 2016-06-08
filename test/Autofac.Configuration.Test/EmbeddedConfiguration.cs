using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Xml;

namespace Autofac.Configuration.Test
{
    public static class EmbeddedConfiguration
    {
        public static IConfiguration LoadJson(string configFile)
        {
            var provider = new JsonConfigurationProvider(new JsonConfigurationSource { Optional = true });
            using (var stream = typeof(EmbeddedConfiguration).GetTypeInfo().Assembly.GetManifestResourceStream("Autofac.Configuration.Test.Files." + configFile))
            {
                provider.Load(stream);
            }

            return new ConfigurationBuilder().Add(provider.Source).Build();
        }

        public static IConfiguration LoadXml(string configFile)
        {
            var provider = new XmlConfigurationProvider(new XmlConfigurationSource { Optional = true });
            using (var stream = typeof(EmbeddedConfiguration).GetTypeInfo().Assembly.GetManifestResourceStream("Autofac.Configuration.Test.Files." + configFile))
            {
                provider.Load(stream);
            }

            return new ConfigurationBuilder().Add(provider.Source).Build();
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
