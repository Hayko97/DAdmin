using DynamicAdmin.Components.Components.Charts.ViewModels;
using DynamicAdmin.Components.Components.ViewModels;
using DynamicAdmin.Components.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace DynamicAdmin.Components.Components;

public partial class Menu
{
    private string _selectedItem;
    private IEnumerable<string> _entityNames;
    [Inject] public IDbInfoService DbInfoService { get; set; }
    [Parameter] public EventCallback<MenuItem> OnSelectedItem { get; set; }
    
    protected override Task OnInitializedAsync()
    {
        _entityNames = DbInfoService.GetEntityNames();

        return Task.CompletedTask;
    }

    private async Task SelectItem(MenuItem selectedItem)
    {
        _selectedItem = selectedItem.Name;
        await OnSelectedItem.InvokeAsync(selectedItem);
        StateHasChanged();
    }
}