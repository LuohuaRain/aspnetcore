// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Components;

/// <summary>
/// Describes the render mode for a component.
/// </summary>
public interface IComponentRenderMode
{
    /// <summary>
    /// Gets a numeric representation of the render mode. This is a framework implementation detail
    /// and should not normally be used in application code.
    /// </summary>
    /// <returns>A numeric representation of the render mode.</returns>
    public static abstract byte AsNumericValue();
}
