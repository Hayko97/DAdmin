using DAdmin.Components.Components;
using DAdmin.Components.Components.Charts.ViewModels;
using DAdmin.Components.Components.Menus.ViewModels;
using DAdmin.Components.Extensions;
using DAdmin.Components.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace DAdmin.Components;

//TODO implement for generic contexts
public partial class AdminPanel
{
    private List<string> _tableNames;

    private RenderFragment _renderedContent;

    private MenuItem _selectedMenuItem;
    [Inject] public DbContext DbContext { get; set; }

    [Parameter] public RenderFragment ChildContent { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (DbContext == null) throw new InvalidOperationException("DbContext is required");

        _tableNames = DbContext.Model.GetEntityTypes().Select(type => type.Name).ToList();
    }

    private async Task OnSelectedItem(MenuItem menuItem)
    {
        _selectedMenuItem = menuItem;
    }
    
    public void AddMenuItem(MenuItem menuItem) 
    {
        // Method that child can call
    }
}