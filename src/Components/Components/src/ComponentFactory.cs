// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Components.Reflection;
using Microsoft.AspNetCore.Components.RenderTree;
using static Microsoft.AspNetCore.Internal.LinkerFlags;

namespace Microsoft.AspNetCore.Components;

internal sealed class ComponentFactory
{
    private const BindingFlags _injectablePropertyBindingFlags
        = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static readonly ConcurrentDictionary<Type, ComponentTypeInfoCacheEntry> _cachedComponentTypeInfo = new();

    private readonly IComponentActivator _componentActivator;
    private readonly Renderer _renderer;

    public ComponentFactory(IComponentActivator componentActivator, Renderer renderer)
    {
        _componentActivator = componentActivator ?? throw new ArgumentNullException(nameof(componentActivator));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
    }

    public static void ClearCache() => _cachedComponentTypeInfo.Clear();

    private static ComponentTypeInfoCacheEntry GetComponentTypeInfo(Type componentType) =>
        _cachedComponentTypeInfo.GetOrAdd(componentType, static ([DynamicallyAccessedMembers(Component)] componentType) =>
            new ComponentTypeInfoCacheEntry(GetDefaultRenderMode(componentType), CreatePropertyInjector(componentType)));

    public IComponent InstantiateComponent(IServiceProvider serviceProvider, [DynamicallyAccessedMembers(Component)] Type componentType, IComponentRenderMode? callerSpecifiedRenderMode)
    {
        var componentTypeInfo = GetComponentTypeInfo(componentType);
        var renderMode = callerSpecifiedRenderMode ?? componentTypeInfo.DefaultRenderMode;

        IComponent component;
        if (renderMode is not null)
        {
            // This is the first point at which we've fully resolved the final render mode, so now we can check
            // how this Renderer wants to handle it
            if (!_renderer.SupportsRenderMode(renderMode, out var usePlaceholder))
            {
                throw new NotSupportedException($"The current renderer does not support the render mode '{renderMode}'.");
            }

            if (usePlaceholder)
            {
                // We swap out the real component type with RenderModePlaceholder so we can emit instructions
                // into the document to instantiate the original component type
                var placeholderComponent = new RenderModePlaceholder(renderMode, componentType);
                componentType = typeof(RenderModePlaceholder);
                componentTypeInfo = GetComponentTypeInfo(componentType);
                component = placeholderComponent;
            }
            else
            {
                component = _componentActivator.CreateInstance(componentType);
            }
        }
        else
        {
            component = _componentActivator.CreateInstance(componentType);
        }

        if (component is null)
        {
            // The default activator will never do this, but an externally-supplied one might
            throw new InvalidOperationException($"The component activator returned a null value for a component of type {componentType.FullName}.");
        }

        componentTypeInfo.PerformPropertyInjection(serviceProvider, component);
        return component;
    }

    private static IComponentRenderMode? GetDefaultRenderMode(Type componentType)
        => componentType.GetCustomAttribute<DefaultRenderModeAttribute>()?.DefaultRenderMode;

    private static Action<IServiceProvider, IComponent> CreatePropertyInjector([DynamicallyAccessedMembers(Component)] Type type)
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

    // Tracks information about a specific component type that ComponentFactory uses
    private record class ComponentTypeInfoCacheEntry(
        IComponentRenderMode? DefaultRenderMode,
        Action<IServiceProvider, IComponent> PerformPropertyInjection);
}
