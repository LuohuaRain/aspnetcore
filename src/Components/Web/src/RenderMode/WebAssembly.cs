// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Components.Web.RenderModes;

/// <summary>
/// A <see cref="IComponentRenderMode"/> indicating that the component should be rendered on the client using WebAssembly.
/// </summary>
public class WebAssembly : IComponentRenderMode, IDefaultComponentRenderMode
{
    static IComponentRenderMode? IDefaultComponentRenderMode.RenderMode => RenderMode.WebAssembly;

    /// <summary>
    /// Constructs an instance of <see cref="WebAssembly"/>.
    /// </summary>
    public WebAssembly() : this(true)
    {
    }

    /// <summary>
    /// Constructs an instance of <see cref="WebAssembly"/>
    /// </summary>
    /// <param name="prerender">A flag indicating whether the component should first prerender on the server. The default value is true.</param>
    public WebAssembly(bool prerender)
    {
        Prerender = prerender;
    }

    /// <summary>
    /// A flag indicating whether the component should first prerender on the server. The default value is true.
    /// </summary>
    public bool Prerender { get; }
}
