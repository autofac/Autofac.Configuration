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
            var config = new ConfigurationBuilder();
            var source = new JsonConfigurationProvider("path", true);
            using (var stream = typeof(EmbeddedConfiguration).GetTypeInfo().Assembly.GetManifestResourceStream("Autofac.Configuration.Test.Files." + configFile))
            {
                typeof(JsonConfigurationProvider).GetMethod("Load", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(source, new object[] { stream });
            }
            config.Add(source, false);
            return config.Build();
        }

        public static IConfiguration LoadXml(string configFile)
        {
            var config = new ConfigurationBuilder();
            var source = new XmlConfigurationProvider("path");
            using (var stream = typeof(EmbeddedConfiguration).GetTypeInfo().Assembly.GetManifestResourceStream("Autofac.Configuration.Test.Files." + configFile))
            {
                typeof(XmlConfigurationProvider).GetMethod("Load", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(source, new object[] { stream });
            }
            config.Add(source, false);
            return config.Build();
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
