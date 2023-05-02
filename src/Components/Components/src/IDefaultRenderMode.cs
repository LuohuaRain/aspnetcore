// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Components;

/// <summary>
/// Supplies the default render mode for a component type, if any.
/// </summary>
public interface IDefaultRenderMode
{
    /// <summary>
    /// Gets the default render mode for a component type, if any.
    /// </summary>
    static abstract IComponentRenderMode? DefaultRenderMode { get; }
}
