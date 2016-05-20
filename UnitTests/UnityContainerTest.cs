using FluentAssertions;
using Microsoft.Practices.Unity;
using NUnit.Framework;

namespace Unity.Patterns
{
    [TestFixture]
    public class UnityContainerTest
    {
        [Test]
        public void TestRegisterByConvention()
        {
            var container = new UnityContainer();

            SetupRegisterByConvention(container);
            container.RegisterInstance(new MyInstance("data"));

            VerifyRegisterByConvention(container);
            container.Resolve<MyInstance>().Data.Should().Be("data");
        }

        [Test]
        public void TestDecoratorManual()
        {
            var container = new UnityContainer();

            SetupDecoratorManual(container);

            VerifyDecorator(container);
        }

        [Test]
        public void TestDecoratorStack()
        {
            var container = new UnityContainer();

            SetupDecoratorStack(container);

            VerifyDecorator(container);
        }

        [Test]
        public void TestRegisterByConventionPlusDecorator()
        {
            var container = new UnityContainer();

            SetupDecoratorStack(container);
            SetupRegisterByConvention(container);

            VerifyDecorator(container);
            VerifyRegisterByConvention(container);
        }

        private static void SetupRegisterByConvention(IUnityContainer container)
        {
            container.RegisterByConvention(new LifetimeMap
            {
                [typeof(MySingleton)] = WithLifetime.ContainerControlled,
                [typeof(MyTransientService)] = WithLifetime.Transient
                // NOTE: Session lifetime is the default
                //[typeof(MySessionService)] = WithLifetime.Hierarchical
            });
        }

        private static void VerifyRegisterByConvention(IUnityContainer container)
        {
            var child1 = container.CreateChildContainer();
            var child2 = container.CreateChildContainer();

            child1.Resolve<IMySingleton>().Should().BeSameAs(child2.Resolve<IMySingleton>(),
                because: "Singletons are shared across child containers.");
            child1.Resolve<IMyTransientService>().Should().NotBeSameAs(child1.Resolve<IMyTransientService>(),
                because: "Transient types are newed up on each resolve.");
            child1.Resolve<IMySessionService>().Should().BeSameAs(child1.Resolve<IMySessionService>(),
                because: "Session types are shared within child containers.");
            child1.Resolve<IMySessionService>().Should().NotBeSameAs(child2.Resolve<IMySessionService>(),
                because: "Session types are not shared across different child containers.");
        }

        private static void SetupDecoratorManual(IUnityContainer container)
        {
            container.AddNewExtension<DecoratorExtension>();
            container.RegisterType<IMyService, MyServiceDecorator2>();
            container.RegisterType<IMyService, MyServiceDecorator1>();
            container.RegisterType<IMyService, MyService>();
        }

        private static void SetupDecoratorStack(IUnityContainer container)
        {
            container.RegisterDecorators(new DecoratorStack<IMyService>()
                .Push<MyService>()
                .Push<MyServiceDecorator1>()
                .Push<MyServiceDecorator2>());
        }

        private static void VerifyDecorator(IUnityContainer container)
        {
            var result = container.Resolve<IMyService>();
            result.Should().BeOfType<MyServiceDecorator2>();

            result = ((MyServiceDecorator2) result).Inner;
            result.Should().BeOfType<MyServiceDecorator1>();

            result = ((MyServiceDecorator1) result).Inner;
            result.Should().BeOfType<MyService>();
        }
    }
}