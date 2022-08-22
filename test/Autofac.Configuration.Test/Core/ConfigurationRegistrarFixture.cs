// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Configuration.Core;
using Microsoft.Extensions.Configuration;

namespace Autofac.Configuration.Test.Core;

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
