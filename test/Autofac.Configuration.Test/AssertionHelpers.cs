using Xunit;

namespace Autofac.Configuration.Test
{
    internal static class AssertionHelpers
    {
        public static void AssertRegistered<TService>(this IComponentContext context, string message = "Expected component was not registered.")
        {
            Assert.True(context.IsRegistered<TService>(), message);
        }

        public static void AssertNotRegistered<TService>(this IComponentContext context, string message = "Component was registered unexpectedly.")
        {
            Assert.False(context.IsRegistered<TService>(), message);
        }

        public static void AssertRegisteredNamed<TService>(this IComponentContext context, string service, string message = "Expected named component was not registered.")
        {
            Assert.True(context.IsRegisteredWithName(service, typeof(TService)), message);
        }

        public static void AssertNotRegisteredNamed<TService>(this IComponentContext context, string service, string message = "Named component was registered unexpectedly.")
        {
            Assert.False(context.IsRegisteredWithName(service, typeof(TService)), message);
        }
    }
}
