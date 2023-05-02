// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Components.Reflection;
using static Microsoft.AspNetCore.Internal.LinkerFlags;

namespace Microsoft.AspNetCore.Components;

internal sealed class ComponentFactory
{
    private const BindingFlags _injectablePropertyBindingFlags
        = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static readonly ConcurrentDictionary<Type, ComponentTypeInfoCacheEntry> _cachedComponentTypeInfo = new();

    private readonly IComponentActivator _componentActivator;

    public ComponentFactory(IComponentActivator componentActivator)
    {
        _componentActivator = componentActivator ?? throw new ArgumentNullException(nameof(componentActivator));
    }

    public static void ClearCache() => _cachedComponentTypeInfo.Clear();

    public IComponent InstantiateComponent(IServiceProvider serviceProvider, [DynamicallyAccessedMembers(Component)] Type componentType, IComponentRenderMode? callerSpecifiedRenderMode)
    {
        var componentTypeInfo = _cachedComponentTypeInfo.GetOrAdd(componentType, static ([DynamicallyAccessedMembers(Component)] componentType) =>
            new ComponentTypeInfoCacheEntry(
                GetDefaultRenderMode(componentType),
                CreateInitializer(componentType)));

        var renderMode = callerSpecifiedRenderMode ?? componentTypeInfo.DefaultRenderMode;
        if (renderMode is not null)
        {
            Console.WriteLine($"For component of type {componentType}, would use rendermode {renderMode}");
        }

        var component = _componentActivator.CreateInstance(componentType);
        if (component is null)
        {
            // The default activator will never do this, but an externally-supplied one might
            throw new InvalidOperationException($"The component activator returned a null value for a component of type {componentType.FullName}.");
        }

        componentTypeInfo.Initializer(serviceProvider, component);
        return component;
    }

    private static IComponentRenderMode? GetDefaultRenderMode(Type componentType)
    {
        // We need to know the default render mode *before* instantiating the component, as we might not be instantiating it at all,
        // depending on the render mode. So IDefaultRenderMode has a static method. Unfortunately the only way to evaluate this from
        // a Type value is through reflection. The following is compatible with trimming and NativeAOT, and the result is cached so
        // the once-per-component-type execution cost does not have much effect.
        // If this was a problem we could require components to declare a generically-typed attribute,
        // e.g., DefaultRenderModeAttribute<T> : DefaultRenderModeAttribute where T: IDefaultRenderMode
        // and then its constructor could read T.DefaultRenderMode and store it on a property. However, that greatly complicates
        // the Razor compiler output and complicates things for anyone trying to implement a component in a .cs file.
        if (componentType.IsAssignableTo(typeof(IDefaultRenderMode)))
        {
            var methodInfo = typeof(DefaultRenderModeEvaluator<>).MakeGenericType(componentType)
                .GetMethod("GetComponentRenderMode", BindingFlags.Static | BindingFlags.Public)!;
            return (IComponentRenderMode?)methodInfo.Invoke(null, null);
        }

        return null;
    }

    private static Action<IServiceProvider, IComponent> CreateInitializer([DynamicallyAccessedMembers(Component)] Type type)
    {
        // Do all the reflection up front
        List<(string name, Type propertyType, PropertySetter setter)>? injectables = null;
        foreach (var property in MemberAssignment.GetPropertiesIncludingInherited(type, _injectablePropertyBindingFlags))
        {
            if (!property.IsDefined(typeof(InjectAttribute)))
            {
                continue;
            }

            injectables ??= new();
            injectables.Add((property.Name, property.PropertyType, new PropertySetter(type, property)));
        }

        if (injectables is null)
        {
            return static (_, _) => { };
        }

        return Initialize;

        // Return an action whose closure can write all the injected properties
        // without any further reflection calls (just typecasts)
        void Initialize(IServiceProvider serviceProvider, IComponent component)
        {
            foreach (var (propertyName, propertyType, setter) in injectables)
            {
                var serviceInstance = serviceProvider.GetService(propertyType);
                if (serviceInstance == null)
                {
                    throw new InvalidOperationException($"Cannot provide a value for property " +
                        $"'{propertyName}' on type '{type.FullName}'. There is no " +
                        $"registered service of type '{propertyType}'.");
                }

                setter.SetValue(component, serviceInstance);
            }
        }
    }

    private class DefaultRenderModeEvaluator<T> where T : IDefaultRenderMode
    {
        public static IComponentRenderMode? GetComponentRenderMode()
            => T.DefaultRenderMode;
    }

    // Tracks information about a specific component type that ComponentFactory uses
    private record class ComponentTypeInfoCacheEntry(
        IComponentRenderMode? DefaultRenderMode,
        Action<IServiceProvider, IComponent> Initializer);
}
