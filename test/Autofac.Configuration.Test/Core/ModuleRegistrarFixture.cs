using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Autofac.Configuration.Test.Core
{
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

        private class ParameterizedModule : Module
        {
            public ParameterizedModule(string message)
            {
                this.Message = message;
            }

            public string Message { get; private set; }

            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterType<SimpleComponent>().WithProperty("Message", this.Message);
            }
        }

        private interface ITestComponent
        {
        }

        private class SimpleComponent : ITestComponent
        {
            public SimpleComponent()
            {
            }

            public SimpleComponent(int input)
            {
                Input = input;
            }

            public bool ABool { get; set; }

            public int Input { get; set; }

            public string Message { get; set; }
        }
    }
}
