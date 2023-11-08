using DynamicAdmin.Components.Components;
using DynamicAdmin.Components.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace DynamicAdmin.Components;

//TODO for generic contexts
public partial class AdminPanel
{
    private List<string> _tableNames;
    private RenderFragment _tableDataComponent;
    private string _selectedMenuItem;
    [Inject] public DbContext DbContext { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (DbContext == null) throw new InvalidOperationException("DbContext is required");

        _tableNames = DbContext.Model.GetEntityTypes().Select(type => type.Name).ToList();
    }

    private void OnSelectedItem(string tableName)
    {
        var entityType = DbContext.GetTypeFromTableName(tableName);

        _selectedMenuItem = tableName;
        if (entityType != null)
        {
            _tableDataComponent = builder =>
            {
                builder.OpenComponent(0, typeof(TableData<>).MakeGenericType(entityType));
                builder.AddAttribute(1, "EntityName", tableName);
                builder.CloseComponent();
            };

            StateHasChanged();
        }
    }
}