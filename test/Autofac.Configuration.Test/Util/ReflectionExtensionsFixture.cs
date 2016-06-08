using System.Reflection;
using Autofac.Configuration.Util;
using Xunit;

namespace Autofac.Configuration.Test.Util
{
    public class ReflectionExtensionsFixture
    {
        [Fact]
        public void TryGetDeclaringProperty_FindsPropertyFromSetterParameter()
        {
            var expected = typeof(HasProperty).GetProperty("Property");
            var setter = expected.GetSetMethod();
            var valueParameter = setter.GetParameters()[0];
            PropertyInfo actual;
            Assert.True(valueParameter.TryGetDeclaringProperty(out actual));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TryGetDeclaringProperty_FailsToFindProperty()
        {
            var valueParameter = typeof(HasProperty).GetMethod("NotSetter").GetParameters()[0];
            PropertyInfo actual;
            Assert.False(valueParameter.TryGetDeclaringProperty(out actual));
            Assert.Null(actual);
        }

        private class HasProperty
        {
            public string NotSetter(string value)
            {
                return value;
            }

            public string Property { get; set; }
        }
    }
}