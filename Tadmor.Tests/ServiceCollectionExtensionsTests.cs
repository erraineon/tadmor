using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tadmor.Core.Extensions;

namespace Tadmor.Tests
{
    [TestClass]
    public class DiscordClientHostedServiceTests
    {

    }
    [TestClass]
    public class ServiceCollectionExtensionsTests
    {
        private record TestOptions(int Foo, string Bar, ulong? NullableUlong);
        
        [TestMethod]
        public void BindConfigurationSection_Records_Works()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["TestOptions:Foo"] = "1",
                    ["TestOptions:Bar"] = "Test",
                })
                .Build();
            var services = Substitute.For<IServiceCollection>();
            services.BindConfigurationSection<TestOptions>(configuration);
            services
                .Received(1)
                .Add(Arg.Is<ServiceDescriptor>(s => (TestOptions) s.ImplementationInstance == new TestOptions(1, "Test", default)));
        }
        
        private interface IFoo { string Bar { get; } }
        
        private class Foo : IFoo
        {
            public string Bar => "test";
        }
        
        private class FooDecorator : IFoo
        {
            private readonly IFoo _decorated;

            public FooDecorator(IFoo decorated)
            {
                _decorated = decorated;
            }

            public string Bar => _decorated.Bar[..^1];
        }

        [TestMethod]
        public void Decorate_Transient_RegisteredType_Works()
        {
            var services = new ServiceCollection();
            services.AddTransient<IFoo, Foo>();
            services.Decorate<IFoo, FooDecorator>();
            var serviceProvider = services.BuildServiceProvider();
            var foo = serviceProvider.GetService<IFoo>();
            Assert.IsNotNull(foo);
            Assert.AreEqual("tes", foo.Bar);
        }

        [TestMethod]
        public void Decorate_Scoped_Factory_Works()
        {
            var services = new ServiceCollection();
            services.AddScoped<IFoo>(_ => new Foo());
            services.Decorate<IFoo, FooDecorator>();
            var serviceProvider = services.BuildServiceProvider();
            var foo = serviceProvider.GetService<IFoo>();
            Assert.IsNotNull(foo);
            Assert.AreEqual("tes", foo.Bar);
        }

        [TestMethod]
        public void Decorate_Singleton_Instance_Works()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IFoo>(new Foo());
            services.Decorate<IFoo, FooDecorator>();
            var serviceProvider = services.BuildServiceProvider();
            var foo = serviceProvider.GetService<IFoo>();
            Assert.IsNotNull(foo);
            Assert.AreEqual("tes", foo.Bar);
        }
    }
}