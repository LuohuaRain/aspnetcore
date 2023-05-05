// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.Components.RenderTree;

/// <summary>
/// Types in the Microsoft.AspNetCore.Components.RenderTree namespace are not recommended for use outside
/// of the Blazor framework. These types will change in a future release.
/// </summary>
internal class RenderModePlaceholder : IComponent
{
    private readonly IComponentRenderMode _renderMode;
    private readonly Type _componentType;
    private RenderHandle _renderHandle;

    public RenderModePlaceholder(IComponentRenderMode renderMode, Type componentType)
    {
        _renderMode = renderMode;
        _componentType = componentType;
    }

    public void Attach(RenderHandle renderHandle)
    {
        _renderHandle = renderHandle;
    }

    public Task SetParametersAsync(ParameterView parameters)
    {
        _renderHandle.Render(builder =>
        {
            builder.OpenElement(0, "blazor-component");
            builder.AddContent(1, $"[TODO: Emit rendering instructions and params for component type {_componentType} in mode {_renderMode}]");
            builder.CloseElement();
        });
        return Task.CompletedTask;
    }
}
