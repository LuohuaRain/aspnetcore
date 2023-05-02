// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Components;

/// <summary>
/// Specifies the default rendering mode for a component.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public abstract class DefaultRenderModeAttribute : Attribute
{
    /// <summary>
    /// Gets the default rendering mode for a component.
    /// </summary>
    public abstract IComponentRenderMode? RenderMode { get; }
}
