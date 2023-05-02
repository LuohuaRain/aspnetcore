// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Components;

/// <summary>
/// Specifies the default rendering mode for a component type.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DefaultRenderModeAttribute<T> : DefaultRenderModeAttribute where T: IComponentRenderMode, new()
{
    public DefaultRenderModeAttribute() : base(new T())
    {
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DefaultRenderModeAttribute : Attribute
{
    public DefaultRenderModeAttribute(IComponentRenderMode renderMode)
    {
        RenderMode = renderMode;
    }

    /// <summary>
    /// Gets the component type's default <see cref="IComponentRenderMode"/> if one is specified.
    /// </summary>
    public IComponentRenderMode? RenderMode { get; }
}
