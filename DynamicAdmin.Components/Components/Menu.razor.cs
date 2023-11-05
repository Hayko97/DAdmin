using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace DynamicAdmin.Components.Components;

public partial class Menu
{
    private string selectedTableName;
    private List<string> tableNames;
    [Inject] public DbContext DbContext { get; set; }
    [Parameter] public EventCallback<string> OnSelectedItem { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (DbContext == null) throw new InvalidOperationException("DbContext is required");

        tableNames = DbContext.Model.GetEntityTypes()
            .Select(type => type.ClrType.Name)
            .ToList();
    }

    private async Task SelectTable(string tableName)
    {
        selectedTableName = tableName;
        await OnSelectedItem.InvokeAsync(tableName);
        StateHasChanged();
    }
}