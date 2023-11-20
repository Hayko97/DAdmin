using DAdmin.Menus.ViewModels;
using DAdmin.States;
using Microsoft.AspNetCore.Components;

namespace DAdmin;

public partial class MenuItem : DAdminComponent
{
    [Parameter] public string Name { get; set; }
    [Parameter] public string IconClass { get; set; }
    [Parameter] public MenuSection MenuSection { get; set; }
    [Parameter] public RenderFragment ChildContent { get; set; }

    [CascadingParameter] public AdminPanel? AdminPanel { get; set; }

    [Inject] private MenuState MenuState { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (ChildContent != null)
        {
            await MenuState.AddMenuItemAsync(new MenuItemModel
            {
                Section = MenuSection,
                Name = Name,
                IconClass = IconClass,
                Content = ChildContent
            });
        }
    }
}