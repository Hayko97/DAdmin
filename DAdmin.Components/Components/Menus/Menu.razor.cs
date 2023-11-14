using DAdmin.Components.Components.Charts.ViewModels;
using DAdmin.Components.Components.Menus.ViewModels;
using DAdmin.Components.Services.DbServices.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace DAdmin.Components.Components.Menus;

public partial class Menu
{
    private MenuItemModel _selectedItemModel = new();

    [Inject] public IDbInfoService DbInfoService { get; set; }
    [Parameter] public EventCallback<MenuItemModel> OnSelectedItem { get; set; }

    [Parameter] public Dictionary<MenuSection, MenuItemModel> MenuItems { get; set; }
    
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override Task OnInitializedAsync()
    {
        return Task.CompletedTask;
    }

    private async Task SelectItem(MenuItemModel selectedItemModel)
    {
        _selectedItemModel = selectedItemModel;
        await OnSelectedItem.InvokeAsync(selectedItemModel);
        StateHasChanged();
    }
}