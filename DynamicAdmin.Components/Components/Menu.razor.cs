using DynamicAdmin.Components.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace DynamicAdmin.Components.Components;

public partial class Menu
{
    private string selectedTableName;
    private IEnumerable<string> tableNames;
    [Inject] public IDbInfoService DbInfoService { get; set; }
    [Parameter] public EventCallback<string> OnSelectedItem { get; set; }

    protected override Task OnInitializedAsync()
    {
        tableNames = DbInfoService.GetEntityNames();

        return Task.CompletedTask;
    }

    private async Task SelectTable(string tableName)
    {
        selectedTableName = tableName;
        await OnSelectedItem.InvokeAsync(tableName);
        StateHasChanged();
    }
}