using Microsoft.AspNetCore.Components;

namespace DAdmin;

public partial class DAdminComponent : ComponentBase
{
    [Parameter] public string Color { get; set; }

    [Parameter] public string Font { get; set; }
    [Parameter] public string Style { get; set; }
    [Parameter] public string Class { get; set; }

    [Inject] public JSInterop JsInterop { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JsInterop.LoadJS("_content/DAdmin/admin.js");
        }
    }
}