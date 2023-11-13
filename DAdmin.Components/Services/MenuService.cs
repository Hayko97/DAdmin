using DAdmin.Components.Components;
using DAdmin.Components.Components.Menus.ViewModels;
using DAdmin.Components.Services.DbServices;
using DAdmin.Components.Services.DbServices.Interfaces;

namespace DAdmin.Components.Services;

public class MenuService : IMenuService
{
    private IDbInfoService _dbInfoService;

    public MenuService(IDbInfoService dbInfoService)
    {
        _dbInfoService = dbInfoService;
    }

    public Task<Dictionary<MenuType, MenuItem>> GetDefaultMenu()
    {
        var menuItems = GetRootMenuItems();

        var entityNames = _dbInfoService.GetEntityTypes();
        foreach (var item in entityNames)
        {
            menuItems[MenuType.Resources].SubItems?.Add(new MenuItem
            {
                Name = item.ClrType.Name,
                Type = MenuType.Resources,
                ComponentType = typeof(TableData<>).MakeGenericType(item.ClrType),
                Parameters = new Dictionary<string, object>()
                {
                    { "ResourceName", item.ClrType.Name },
                },
                SubItems = null
            });
        }

        return Task.FromResult(menuItems);
    }

    public Dictionary<MenuType, MenuItem> GetRootMenuItems()
    {
        var menuItems = new Dictionary<MenuType, MenuItem>();

        menuItems[MenuType.Dashboard] = new MenuItem()
        {
            Name = MenuType.Dashboard.ToString(),
            IconClass = "fa fa-home",
            ComponentType = typeof(Dashboard),
            SubItems = null
        };

        menuItems[MenuType.Resources] = new MenuItem()
        {
            Name = MenuType.Resources.ToString(),
            ComponentType = null,
            IconClass = "fa fa-table",
            SubItems = new List<MenuItem>(),
        };

        menuItems[MenuType.Charts] = new MenuItem()
        {
            Name = MenuType.Charts.ToString(),
            ComponentType = null,
            IconClass = "fa fa-bar-chart",
            SubItems = new List<MenuItem>(),
        };

        menuItems[MenuType.Stats] = new MenuItem()
        {
            Name = MenuType.Stats.ToString(),
            ComponentType = null,
            IconClass = "far fa-calendar-alt",
            SubItems = new List<MenuItem>(),
        };

        return menuItems;
    }
}