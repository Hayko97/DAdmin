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
    [Parameter] public bool UseContextEntities { get; set; }

    private bool _stateInitialized { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Dictionary<MenuSection, MenuItemModel> menuItems = MenuService.GetRootMenuItems();

        if (ChildContent == null)
        {
            menuItems = await MenuService.AddDefaultMenuItems(menuItems);
        }
        else if (UseContextEntities)
        {
            menuItems = await MenuService.AddEntitiesToResources(menuItems);
        }

        MenuState.MenuItems = menuItems;
        _stateInitialized = true;
    }

    private Task OnSelectedItem(MenuItemModel menuItemModel)
    {
        if (menuItemModel.Content != null)
        {
            _renderedContent = menuItemModel.Content;
        }
        else
        {
            _renderedContent = menuItemModel.RenderContent();
            StateHasChanged();
        }

        return Task.CompletedTask;
    }
}