// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Components.Web.RenderMode;

/// <summary>
/// Represents a render mode in which components first prerender on the server, then run on WebAssembly.
/// </summary>
public class WebAssemblyPrerendered : IComponentRenderMode
{
    static byte IComponentRenderMode.AsNumericValue() => RenderModes.WebAssemblyPrerendered;

    /// <summary>
    /// Gets an instance of the <see cref="WebAssemblyPrerendered"/> type.
    /// Caution: This is a temporary API for .NET 8 preview releases until the Razor compiler is updated to support the @rendermode directive attribute.
    /// </summary>
    public static WebAssemblyPrerendered Instance { get; } = new();
}
