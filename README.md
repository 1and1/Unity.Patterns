# Unity.Patterns

Unity.Patterns provides extensions for [Unity](https://github.com/unitycontainer/unity) dependency injection framework. The main focus is on Registration by Convention and the Decorator pattern.

NuGet package:
* [Unity.Patterns](https://www.nuget.org/packages/Unity.Patterns/)



## Registration by Convention

A common way to use Unity's support for [Register by Convention](https://msdn.microsoft.com/en-us/library/dn507479(v=pandp.30).aspx) looks like this:
```cs
container.RegisterTypes(
    AllClasses.FromAssembliesInBasePath(),
    WithMapping.MatchingInterface,
    WithName.Default,
    WithLifetime.Hierarchical);
```
This library provides a replacement for `AllClasses.FromAssembliesInBasePath()` called `TypeLocator.FromAssembliesInSearchPath()`. Unlike the Unity method this respects `AppDomain.CurrentDomain.RelativeSearchPath` if set. This is important for ASP.NET applications where the binaries are commonly located in a subdirectory called `bin`.

This library also provides `LifetimeMap`, an alternative to manually creating lambdas that build `LifetimeManager`s:
```cs
var map = new LifetimeMap(defaultLimetime: WithLifetime.Hierarchical)
    .Add<MySingleton>(WithLifetime.ContainerControlled)
    .Add<MyTransientService>(WithLifetime.Transient);
container.RegisterTypes(
    AllClasses.FromAssembliesInBasePath(),
    WithMapping.MatchingInterface,
    WithName.Default,
    map);
```

Alternatively, you can also use the C# 6.0 dictionary initialization syntax:
```cs
var map = new LifetimeMap(defaultLimetime: WithLifetime.Hierarchical)
{
    [typeof(MySingleton)] = WithLifetime.ContainerControlled,
    [typeof(MyTransientService)] = WithLifetime.Transient
};
```

This library provides an alternative extension method to perform Registration by Convention, that hardcodes to `WithMapping.MatchingInterface` and `WithName.Default` and defaults to `WithLifetime.Hierarchical`:
```cs
container.RegisterByConvention();
```

This can be combined with `LifetimeMap`:
```cs
container.RegisterByConvention(new LifetimeMap
{
    [typeof(MySingleton)] = WithLifetime.ContainerControlled,
    [typeof(MyTransientService)] = WithLifetime.Transient
});
```



## Decorator pattern

The [Decorator pattern](https://en.wikipedia.org/wiki/Decorator_pattern) allows you to wrap one or more intercepting layers around an object that all implement the same interface.

After adding the `DecoratorExtension` to your Unity container you can register multiple implementations for a single interface. Start from outermost and move inwards:
```cs
container.AddNewExtension<DecoratorExtension>();
container.RegisterType<IMyService, MyServiceDecorator2>();
container.RegisterType<IMyService, MyServiceDecorator1>();
container.RegisterType<IMyService, MyService>();
```

The extension method `.RegisterDecorators()` and the class `.DecoratorStack<>` provide a more declarative alternative. Start from innermost and move outwards:
```cs
container.RegisterDecorators(new DecoratorStack<IMyService>()
    .Push<MyService>()
    .Push<MyServiceDecorator1>()
    .Push<MyServiceDecorator2>());
```

IMPORTANT: When combining the Decorator feature with Registration by Convention the Decorator configuration MUST be applied to the Unity container first in order to work properly!
