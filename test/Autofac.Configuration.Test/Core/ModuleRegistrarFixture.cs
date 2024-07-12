// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core.Activators.Reflection;

namespace Autofac.Configuration.Test.Core;

public class ModuleRegistrarFixture
{
    [Fact]
    public void RegisterConfiguredModules_AllowsMultipleModulesOfSameTypeWithDifferentParameters()
    {
        // Issue #271: Could not register more than one module with the same type but different parameters in XmlConfiguration.
        var builder = EmbeddedConfiguration.ConfigureContainerWithJson("ModuleRegistrar_SameModuleRegisteredMultipleTimes.json");
        var container = builder.Build();
        var collection = container.Resolve<IEnumerable<SimpleComponent>>();
        Assert.Equal(2, collection.Count());

        // Test using Any() because we aren't necessarily guaranteed the order of resolution.
        Assert.True(collection.Any(a => a.Message == "First"), "The first registration wasn't found.");
        Assert.True(collection.Any(a => a.Message == "Second"), "The second registration wasn't found.");
    }

    [Fact]
    public void RegisterConfiguredModules_ConstructsModulesUsingTheBestAvailableConstructor()
    {
        // Issue #44: Loading new modules via ConfigurationRegistrar always constructs modules using the constructor with most parameters.
        var builder = EmbeddedConfiguration.ConfigureContainerWithJson("ModuleRegistrar_SameModuleWithVaryingConstructorParams.json");
        var container = builder.Build();
        var collection = container.Resolve<IEnumerable<AnotherSimpleComponent>>();
        Assert.Equal(4, collection.Count());

        Assert.Equal(1, collection.Count(component => component.ABool != null && component.Input == null && component.Message == null));
        Assert.Equal(1, collection.Count(component => component.ABool != null && component.Input != null && component.Message == null));
        Assert.Equal(1, collection.Count(component => component.ABool != null && component.Input != null && component.Message != null));
        Assert.Equal(1, collection.Count(component => component.ABool == null && component.Input == null && component.Message == null)); // Fallback case
    }

    [Fact]
    public void RegisterConfiguredComponents_MetadataMissingName_ThrowsInvalidOperation()
    {
        var builder = EmbeddedConfiguration.ConfigureContainerWithXml("ModuleRegistrar_ModulesMissingName.xml");
        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());
        Assert.Equal("The 'modules' collection should be ordinal (like an array) with items that have numeric names to indicate the index in the collection. 'modules' didn't have a numeric name so couldn't be parsed. Check https://autofac.readthedocs.io/en/latest/configuration/xml.html for configuration examples.", exception.Message);
    }

    [Fact]
    public void RegisterConfiguredComponents_ModuleWithNoPublicConstructor_ThrowsInvalidOperation()
    {
        var builder = EmbeddedConfiguration.ConfigureContainerWithJson("ModuleRegistrar_ModuleWithNoPublicConstructor.json");
        Assert.Throws<NoConstructorsFoundException>(() => builder.Build());
    }

    [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through configuration.")]
    private class ParameterizedModule : Module
    {
        public ParameterizedModule(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SimpleComponent>().WithProperty(nameof(Message), Message);
        }
    }

    [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through configuration.")]
    private class ModuleWithMultipleValidConstructors : Module
    {
        public ModuleWithMultipleValidConstructors(bool? aBool)
        {
            ABool = aBool;
        }

        public ModuleWithMultipleValidConstructors(bool? aBool, double? input)
        {
            ABool = aBool;
            Input = input;
        }

        public ModuleWithMultipleValidConstructors(bool? aBool, double? input, string message)
        {
            ABool = aBool;
            Input = input;
            Message = message;
        }

        public bool? ABool { get; set; }

        public double? Input { get; set; }

        public string Message { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AnotherSimpleComponent>()
                   .WithProperty(nameof(ABool), ABool)
                   .WithProperty(nameof(Input), Input)
                   .WithProperty(nameof(Message), Message);
        }
    }

    [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through configuration.")]
    private class ProtectedModule : Module
    {
        protected ProtectedModule(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }
    }

    private interface ITestComponent
    {
    }

    [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through configuration.")]
    private class SimpleComponent : ITestComponent
    {
        public SimpleComponent()
        {
        }

        public SimpleComponent(double input)
        {
            Input = input;
        }

        public bool ABool { get; set; }

        public double Input { get; set; }

        public string Message { get; set; }
    }

    [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through configuration.")]
    private class AnotherSimpleComponent : ITestComponent
    {
        public AnotherSimpleComponent()
        {
        }

        public bool? ABool { get; set; }

        public double? Input { get; set; }

        public string Message { get; set; }
    }
}
