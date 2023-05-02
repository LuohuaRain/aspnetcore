// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Components.Web.RenderMode;

/// <summary>
/// Represents a render mode in which components first prerender on the server, then run on the server via a WebSocket connection.
/// </summary>
public class ServerPrerendered : IComponentRenderMode
{
    static byte IComponentRenderMode.AsNumericValue() => RenderModes.ServerPrerendered;

    /// <summary>
    /// Gets an instance of the <see cref="ServerPrerendered"/> type.
    /// Caution: This is a temporary API for .NET 8 preview releases until the Razor compiler is updated to support the @rendermode directive attribute.
    /// </summary>
    public static ServerPrerendered Instance { get; } = new();
}
