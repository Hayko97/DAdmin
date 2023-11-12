using DAdmin.Components.Components.Charts.ViewModels;
using DAdmin.Components.Components.ViewModels;
using DAdmin.Components.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace DAdmin.Components.Components;

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