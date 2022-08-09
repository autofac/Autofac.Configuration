﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Autofac.Configuration.Test.Core
{
    public class ModuleConfiguration_ComplexTypeFixture
    {
        [Fact]
        public void RegisterConfiguredModules_ComplexParameterType()
        {
            var builder = EmbeddedConfiguration.ConfigureContainerWithJson("ModuleConfiguration_ComplexType.json");
            var container = builder.Build();

            var poco = container.Resolve<ComplexParameterComponent>();

            Assert.Equal(2, poco.List.Count);
            Assert.Equal("Val1", poco.List[0]);
            Assert.Equal("Val2", poco.List[1]);
        }

        [Fact]
        public void RegisterConfiguredModules_ComplexPropertyType()
        {
            var builder = EmbeddedConfiguration.ConfigureContainerWithJson("ModuleConfiguration_ComplexType.json");
            var container = builder.Build();

            var poco = container.Resolve<ComplexPropertyComponent>();

            Assert.Equal(2, poco.List.Count);
            Assert.Equal("Val3", poco.List[0]);
            Assert.Equal("Val4", poco.List[1]);
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through configuration.")]
        private class ComplexParameterTypeModule : Module
        {
            public ComplexType ComplexType { get; set; }

            public ComplexParameterTypeModule(ComplexType complexType)
            {
                ComplexType = complexType;
            }

            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterType<ComplexParameterComponent>().WithProperty("List", ComplexType.List);
            }
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through configuration.")]
        private class ComplexPropertyTypeModule : Module
        {
            public ComplexType ComplexType { get; set; }

            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterType<ComplexPropertyComponent>().WithProperty("List", ComplexType.List);
            }
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through configuration.")]
        private class ComplexType
        {
            public IList<string> List { get; set; }
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through configuration.")]
        private class ComplexParameterComponent
        {
            public IList<string> List { get; set; }
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through configuration.")]
        private class ComplexPropertyComponent
        {
            public IList<string> List { get; set; }
        }
    }
}
