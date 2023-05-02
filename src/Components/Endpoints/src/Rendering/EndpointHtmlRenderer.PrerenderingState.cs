// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Components.Infrastructure;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Components.Endpoints;

internal partial class EndpointHtmlRenderer
{
    private static readonly object InvokedRenderModesKey = new object();

    public async ValueTask<IHtmlContent> PrerenderPersistedStateAsync(HttpContext httpContext, PersistedStateSerializationMode serializationMode)
    {
        // First we resolve "infer" mode to a specific mode
        if (serializationMode == PersistedStateSerializationMode.Infer)
        {
            switch (GetPersistStateRenderMode(httpContext))
            {
                case InvokedRenderModes.Mode.None:
                    return ComponentStateHtmlContent.Empty;
                case InvokedRenderModes.Mode.ServerAndWebAssembly:
                    throw new InvalidOperationException(
                        Resources.FailedToInferComponentPersistenceMode);
                case InvokedRenderModes.Mode.Server:
                    serializationMode = PersistedStateSerializationMode.Server;
                    break;
                case InvokedRenderModes.Mode.WebAssembly:
                    serializationMode = PersistedStateSerializationMode.WebAssembly;
                    break;
                default:
                    throw new InvalidOperationException("Invalid persistence mode");
            }
        }

        // Now given the mode, we obtain a particular store for that mode
        var store = serializationMode switch
        {
            PersistedStateSerializationMode.Server =>
                new ProtectedPrerenderComponentApplicationStore(httpContext.RequestServices.GetRequiredService<IDataProtectionProvider>()),
            PersistedStateSerializationMode.WebAssembly =>
                new PrerenderComponentApplicationStore(),
            _ =>
                throw new InvalidOperationException("Invalid persistence mode.")
        };

        // Finally, persist the state and return the HTML content
        var manager = httpContext.RequestServices.GetRequiredService<ComponentStatePersistenceManager>();
        await manager.PersistStateAsync(store, Dispatcher);
        return new ComponentStateHtmlContent(store);
    }

    // Internal for test only
    internal static void UpdateSaveStateRenderMode(HttpContext httpContext, IComponentRenderMode mode)
    {
        // TODO: This will all have to change when we support multiple render modes in the same response
        if (mode is ServerRenderMode { Prerender: true } || mode is WebAssemblyRenderMode { Prerender: true })
        {
            var currentInvocation = mode is ServerRenderMode
                ? InvokedRenderModes.Mode.Server
                : InvokedRenderModes.Mode.WebAssembly;

            if (!httpContext.Items.TryGetValue(InvokedRenderModesKey, out var result))
            {
                httpContext.Items[InvokedRenderModesKey] = new InvokedRenderModes(currentInvocation);
            }
            else
            {
                var invokedMode = (InvokedRenderModes)result!;
                if (invokedMode.Value != currentInvocation)
                {
                    invokedMode.Value = InvokedRenderModes.Mode.ServerAndWebAssembly;
                }
            }
        }
    }

    internal static InvokedRenderModes.Mode GetPersistStateRenderMode(HttpContext httpContext)
    {
        return httpContext.Items.TryGetValue(InvokedRenderModesKey, out var result)
            ? ((InvokedRenderModes)result!).Value
            : InvokedRenderModes.Mode.None;
    }

    private sealed class ComponentStateHtmlContent : IHtmlContent
    {
        private PrerenderComponentApplicationStore? _store;

        public static ComponentStateHtmlContent Empty { get; }
            = new ComponentStateHtmlContent(null);

        public ComponentStateHtmlContent(PrerenderComponentApplicationStore? store)
        {
            _store = store;
        }

        public void WriteTo(TextWriter writer, HtmlEncoder encoder)
        {
            if (_store != null)
            {
                writer.Write("<!--Blazor-Component-State:");
                writer.Write(_store.PersistedState);
                writer.Write("-->");
                _store = null;
            }
        }
    }
}
