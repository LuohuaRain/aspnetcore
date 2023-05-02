// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Components.Web;

internal static class RenderModes
{
    public const byte Unspecified = 0;
    public const byte Server = 1;
    public const byte ServerPrerendered = 2;
    public const byte WebAssembly = 3;
    public const byte WebAssemblyPrerendered = 4;
}
