using System;
using System.Collections.Generic;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;

namespace Unity.Patterns
{
    /// <summary>
    /// Adds support for the Decorator pattern to Unity.
    /// </summary>
    /// <remarks>Multiple <see cref="IUnityContainer.RegisterType"/> calls for the same interface with different implementations are used to indicate decorators. Start from outermost and move inwards.</remarks>
    public class DecoratorExtension : UnityContainerExtension
    {
        private readonly DecoratorBuildStrategy _strategy = new DecoratorBuildStrategy();

        protected override void Initialize()
        {
            Context.Registering += _strategy.AddRegistration;
            Context.Strategies.Add(_strategy, UnityBuildStage.PreCreation);
        }

        public override void Remove()
        {
            Context.Registering -= _strategy.AddRegistration;
        }

        private class DecoratorBuildStrategy : BuilderStrategy
        {
            private readonly Dictionary<Type, Stack<Type>> _stackDictionary = new Dictionary<Type, Stack<Type>>();

            public override void PreBuildUp(IBuilderContext context)
            {
                var key = context.OriginalBuildKey;
                if (!key.Type.IsInterface || context.GetOverriddenResolver(key.Type) != null) return;

                Stack<Type> stack;
                if (!_stackDictionary.TryGetValue(key.Type, out stack)) return;

                object value = null;
                foreach (var t in stack)
                {
                    value = context.NewBuildUp(new NamedTypeBuildKey(t, key.Name));
                    var overrides = new DependencyOverride(key.Type, value);
                    context.AddResolverOverrides(overrides);
                }

                context.Existing = value;
                context.BuildComplete = true;
            }

            public void AddRegistration(object sender, RegisterEventArgs e)
            {
                if (e.TypeFrom == null || !e.TypeFrom.IsInterface) return;

                Stack<Type> stack;
                if (_stackDictionary.ContainsKey(e.TypeFrom))
                    stack = _stackDictionary[e.TypeFrom];
                else
                {
                    stack = new Stack<Type>();
                    _stackDictionary.Add(e.TypeFrom, stack);
                }

                if (!stack.Contains(e.TypeTo))
                    stack.Push(e.TypeTo);
            }
        }
    }
}
