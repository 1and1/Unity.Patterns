using System;
using System.Collections.Generic;

namespace Unity.Patterns
{
    /// <summary>
    /// A stack of implementations of an interface for use as a stack of decorators.
    /// </summary>
    /// <typeparam name="TInterface">The base interface of the implementations.</typeparam>
    public class DecoratorStack<TInterface>
    {
        private readonly Stack<Type> _stack = new Stack<Type>();

        internal IEnumerable<Type> Stack => _stack;

        /// <summary>
        /// Adds an implementation to the stack. Start from innermost and move outwards.
        /// </summary>
        /// <typeparam name="TImplementation">The type of the implementation to add.</typeparam>
        /// <returns><code>this</code> for a fluent-style builder interface.</returns>
        public DecoratorStack<TInterface> Push<TImplementation>()
            where TImplementation : TInterface
        {
            _stack.Push(typeof(TImplementation));
            return this;
        }
    }
}