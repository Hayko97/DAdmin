using DAdmin;
using DAdmin.Charts.ViewModels;
using DAdmin.Menus.ViewModels;
using DAdmin.Extensions;
using DAdmin.Helpers;
using DAdmin.Services;
using DAdmin.States;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace DAdmin;

//TODO implement for generic contexts
public partial class AdminPanel : DAdminComponent
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