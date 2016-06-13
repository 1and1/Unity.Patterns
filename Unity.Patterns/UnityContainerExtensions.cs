using System;
using System.Runtime.Serialization;
using System.Security;
using Microsoft.Practices.Unity;

namespace Unity.Patterns
{
    /// <summary>
    /// Provides extension methods for <see cref="IUnityContainer"/>.
    /// </summary>
    public static class UnityContainerExtensions
    {
        /// <summary>
        /// Performs Registration by Convention using <see cref="TypeLocator.FromAssembliesInSearchPath"/>.
        /// </summary>
        /// <param name="container">The container to configure.</param>
        /// <param name="getLifetimeManager">A function that gets the <see cref="LifetimeManager" /> for the registration of each type. It can be an instance of <see cref="LifetimeMap"/> or a method from <see cref="WithLifetime"/>. Defaults to <see cref="WithLifetime.Hierarchical"/>.</param>
        /// <returns><code>this</code> for a fluent-style builder interface.</returns>
        /// <remarks>When combining this with <see cref="RegisterDecorators{TInterface}"/> call this method last.</remarks>
        public static IUnityContainer RegisterByConvention(this IUnityContainer container,
            Func<Type, LifetimeManager> getLifetimeManager = null)
        {
            return container.RegisterTypes(
                TypeLocator.FromAssembliesInSearchPath(),
                WithMappings.FromMatchingInterface,
                WithName.Default,
                getLifetimeManager ?? WithLifetime.Hierarchical);
        }

        /// <summary>
        /// Registers a set of implementations with a common interface according to the Decorator pattern.
        /// </summary>
        /// <typeparam name="TInterface">The base interface of the implementations.</typeparam>
        /// <param name="container">The container to configure.</param>
        /// <param name="decoratorStack">A list of implementations of an interface for use as a stack of decorators starting from inner most moving outwards.</param>
        /// <param name="getLifetimeManager">A function that gets the <see cref="LifetimeManager" /> for the registration of each type. It can be an instance of <see cref="LifetimeMap"/> or a method from <see cref="WithLifetime"/>. Defaults to <see cref="WithLifetime.Hierarchical"/>.</param>
        /// <returns><code>this</code> for a fluent-style builder interface.</returns>
        /// <remarks>When combining this with <see cref="RegisterByConvention"/> call this method first.</remarks>
        public static IUnityContainer RegisterDecorators<TInterface>(this IUnityContainer container,
            DecoratorStack<TInterface> decoratorStack, Func<Type, LifetimeManager> getLifetimeManager = null)
        {
            if (!container.IsRegistered<DecoratorExtension>())
                container.AddNewExtension<DecoratorExtension>();

            foreach (var type in decoratorStack.Stack)
                container.RegisterType(typeof(TInterface), type, (getLifetimeManager ?? WithLifetime.Hierarchical)(type));

            return container;
        }

        /// <summary>
        /// Resolve an instance of the default requested type from the container.
        /// When resolution fails throws the original (inner) exception instead of <see cref="ResolutionFailedException"/>.
        /// </summary>
        public static T ResolveUnwrapped<T>(this IUnityContainer container)
        {
            try
            {
                return container.Resolve<T>();
            }
            catch (ResolutionFailedException ex)
            {
                var exception = ex.InnerException;

                // Try to preserve original stack trace
                var serializationInfo = new SerializationInfo(exception.GetType(), new FormatterConverter());
                var streamingContext = new StreamingContext(StreamingContextStates.CrossAppDomain);
                exception.GetObjectData(serializationInfo, streamingContext);
                try
                {
                    var objectManager = new ObjectManager(null, streamingContext);
                    objectManager.RegisterObject(exception, 1, serializationInfo);
                    objectManager.DoFixups();
                }
                catch (SecurityException)
                {}
                catch (SerializationException)
                {}

                throw exception;
            }
        }
    }
}