using DAdmin.Components.Components.Charts.ViewModels;
using DAdmin.Components.Components.Menus.ViewModels;
using DAdmin.Components.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace DAdmin.Components.Components.Menus;

public partial class Menu
{
    private MenuItem _selectedItem;
    private IEnumerable<string> _entityNames;
    [Inject] public IDbInfoService DbInfoService { get; set; }
    [Parameter] public EventCallback<MenuItem> OnSelectedItem { get; set; }
    
    [Parameter] public List<MenuItem> MenuItems { get; set; }
    
    protected override Task OnInitializedAsync()
    {
        _entityNames = DbInfoService.GetEntityNames();

        return Task.CompletedTask;
    }

    private async Task SelectItem(MenuItem selectedItem)
    {
        _selectedItem = selectedItem;
        await OnSelectedItem.InvokeAsync(selectedItem);
        StateHasChanged();
    }
}