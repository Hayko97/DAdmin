using DAdmin.Components.Components;
using DAdmin.Components.Components.Charts.ViewModels;
using DAdmin.Components.Components.Menus.ViewModels;
using DAdmin.Components.Extensions;
using DAdmin.Components.Helpers;
using DAdmin.Components.Services;
using DAdmin.Components.States;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace DAdmin.Components;

//TODO implement for generic contexts
public partial class AdminPanel
{
    private RenderFragment _renderedContent;
    [Inject] public MenuState MenuState { get; set; }
    [Inject] public IMenuService MenuService { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public bool UseEntitiesAsResource { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Dictionary<MenuType, MenuItem> menuItems = MenuService.GetRootMenuItems();

        if (ChildContent == null)
        {
            MenuState.MenuItems = await MenuService.AddDefaultMenuItems(menuItems);
        }
        else if (UseEntitiesAsResource)
        {
            MenuState.MenuItems = await MenuService.AddEntitiesToResources(menuItems);
        }
    }

    private Task OnSelectedItem(MenuItem menuItem)
    {
        _renderedContent = menuItem.RenderContent();
        StateHasChanged();

        return Task.CompletedTask;
    }
}