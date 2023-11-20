using Microsoft.JSInterop;

namespace DAdmin;

// This class provides an example of how JavaScript functionality can be wrapped
// in a .NET class for easy consumption. The associated JavaScript module is
// loaded on demand when first needed.
//
// This class can be registered as scoped DI service and then injected into Blazor
// components for use.

public class JSInterop : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    public JSInterop(IJSRuntime jsRuntime)
    {
        moduleTask = new Lazy<Task<IJSObjectReference>>(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/DAdmin/admin.js").AsTask());
    }

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }

    public async ValueTask LoadCSS(string url)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("loadCSS", url);
    }

    public async ValueTask LoadJS(string url)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync("loadJS", url);
    }

    public async ValueTask InvokeVoidAsync(string identifier, params object?[]? args)
    {
        var module = await moduleTask.Value;
        await module.InvokeVoidAsync(identifier, args);
    }
}