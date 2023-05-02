// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Components.Web;

/// <summary>
/// Provides pre-constructed <see cref="IComponentRenderMode"/> instances that may be used during rendering.
/// </summary>
public static class RenderMode
{
    /// <summary>
    /// Gets an <see cref="IComponentRenderMode"/> that represents rendering interactively on the server via Blazor Server hosting
    /// with server-side prerendering.
    /// </summary>
    public static ServerRenderMode Server { get; } = new ServerRenderMode();

    /// <summary>
    /// Gets an <see cref="IComponentRenderMode"/> that represents rendering interactively on the client via Blazor WebAssembly hosting
    /// with server-side prerendering.
    /// </summary>
    public static WebAssemblyRenderMode WebAssembly { get; } = new WebAssemblyRenderMode();
}
