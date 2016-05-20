using System;
using System.Collections.Generic;
using Microsoft.Practices.Unity;

namespace Unity.Patterns
{
    /// <summary>
    /// A dictionary mapping <see cref="Type"/>s to <see cref="LifetimeManager"/>s.
    /// </summary>
    /// <remarks>This is to configure Registration by Convention.</remarks>
    public class LifetimeMap : Dictionary<Type, Func<Type, LifetimeManager>>
    {
        private readonly Func<Type, LifetimeManager> _defaultLifetime;

        /// <summary>
        /// Creates a new lifetime map.
        /// </summary>
        /// <param name="defaultLifetime">The default <see cref="LifetimeManager"/> provider to use when there is no matching entry for a <see cref="Type"/>. Leave <c>null</c> to default to <see cref="WithLifetime.Hierarchical"/>.</param>
        public LifetimeMap(Func<Type, LifetimeManager> defaultLifetime = null)
        {
            _defaultLifetime = defaultLifetime ?? WithLifetime.Hierarchical;
        }

        /// <summary>
        /// Adds a new lifetime mapping.
        /// </summary>
        /// <typeparam name="T">The type to add the mapping for.</typeparam>
        /// <param name="lifetime">The <see cref="LifetimeManager"/> provider to call for the specified type. Usually a method from <see cref="WithLifetime"/>.</param>
        /// <returns><code>this</code> for a fluent-style builder interface.</returns>
        public LifetimeMap Add<T>(Func<Type, LifetimeManager> lifetime)
        {
            Add(typeof(T), lifetime);
            return this;
        }

        /// <summary>
        /// Transparently converts the dictionary into a lifetime mapping/lookup funciton.
        /// </summary>
        public static implicit operator Func<Type, LifetimeManager>(LifetimeMap map)
        {
            return map.Resolve;
        }

        private LifetimeManager Resolve(Type type)
        {
            Func<Type, LifetimeManager> lifetime;
            return TryGetValue(type, out lifetime)
                ? lifetime(type)
                : _defaultLifetime(type);
        }
    }
}