using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DAdmin;

public partial class TextContentColumn<TEntity> : DAdminComponent where TEntity : class
{
    [Parameter] public string Name { get; set; }
    [Parameter] public Expression<Func<TEntity, object>> FieldSelector { get; set; }

    [Inject] public IJSRuntime JSRuntime { get; set; }

    private string editorId = $"quill-editor-{Guid.NewGuid()}";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JSRuntime.InvokeVoidAsync("quillFunctions.createQuill", editorId);
        }
    }

    public async Task<string> GetContentAsync()
    {
        return await JSRuntime.InvokeAsync<string>("quillFunctions.getQuillContent", editorId);
    }

    public async Task SetContentAsync(string content)
    {
        await JSRuntime.InvokeVoidAsync("quillFunctions.setQuillContent", editorId, content);
    }
}