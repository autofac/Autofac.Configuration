using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Builder;
using Autofac.Configuration;
using Autofac.Configuration.Core;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Microsoft.Framework.ConfigurationModel;
using Moq;
using Xunit;

namespace Autofac.Configuration.Test.Core
{
    public class ConfigurationRegistrarFixture
    {
        [Fact]
        public void RegisterConfiguration_NullBuilder()
        {
            var configuration = new Mock<IConfiguration>();
            var registrar = new ConfigurationRegistrar();
            Assert.Throws<ArgumentNullException>(() => registrar.RegisterConfiguration(null, configuration.Object));
        }

        [Fact]
        public void RegisterConfiguration_NullConfiguration()
        {
            var builder = new ContainerBuilder();
            var registrar = new ConfigurationRegistrar();
            Assert.Throws<ArgumentNullException>(() => registrar.RegisterConfiguration(builder, null));
        }
    }
}
