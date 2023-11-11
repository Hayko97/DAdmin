using DynamicAdmin.Components.Components;
using DynamicAdmin.Components.Components.Charts.ViewModels;
using DynamicAdmin.Components.Components.ViewModels;
using DynamicAdmin.Components.Extensions;
using DynamicAdmin.Components.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace DynamicAdmin.Components;

//TODO implement for generic contexts
public partial class AdminPanel
{
    private List<string> _tableNames;

    private RenderFragment _renderedContent;

    private MenuItem _selectedMenuItem;
    [Inject] public DbContext DbContext { get; set; }

    [Parameter] public IEnumerable<IChart> Charts { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (DbContext == null) throw new InvalidOperationException("DbContext is required");

        _tableNames = DbContext.Model.GetEntityTypes().Select(type => type.Name).ToList();
    }

    private async Task OnSelectedItem(MenuItem menuItem)
    {
        _selectedMenuItem = menuItem;
        switch (menuItem.MenuItemType)
        {
            case MenuItemType.Dashboard:
                await ShowDashboard(menuItem);
                break;
            case MenuItemType.Entity:
                await ShowTableData(menuItem.Name);
                break;
        }
    }

    private Task ShowTableData(string name)
    {
        var entityType = DbContext.GetTypeFromTableName(name);

        if (entityType != null)
        {
            _renderedContent = builder =>
            {
                builder.OpenComponent(0, typeof(TableData<>).MakeGenericType(entityType));
                builder.AddAttribute(1, "EntityName", name);
                builder.CloseComponent();
            };

            StateHasChanged();
        }

        return Task.CompletedTask;
    }

    private Task ShowDashboard(MenuItem menuItem)
    {
        _renderedContent = builder =>
        {
            builder.OpenComponent(0, typeof(Dashboard));
            builder.AddAttribute(1, "Charts", Charts);
            builder.CloseComponent();
        };

        StateHasChanged();

        return Task.CompletedTask;
    }
}