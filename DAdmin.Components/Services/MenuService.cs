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

    public async Task<Dictionary<MenuSection, MenuItemModel>> AddDefaultMenuItems(Dictionary<MenuSection, MenuItemModel> menuItems)
    {
        menuItems = await AddEntitiesToResources(menuItems);

        //TODO implement for other root menu items
        return menuItems;
    }

    public Task<Dictionary<MenuSection, MenuItemModel>> AddEntitiesToResources(Dictionary<MenuSection, MenuItemModel> menuItems)
    {
        var entityNames = _dbInfoService.GetEntityTypes();
        foreach (var item in entityNames)
        {
            menuItems[MenuSection.Resources].SubItems?.Add(new MenuItemModel
            {
                Name = item.ClrType.Name,
                Section = MenuSection.Resources,
                ComponentType = typeof(DataResource<>).MakeGenericType(item.ClrType),
                Parameters = new Dictionary<string, object>()
                {
                    { "ResourceName", item.ClrType.Name },
                },
                SubItems = null
            });
        }

        return Task.FromResult(menuItems);
    }

    public Dictionary<MenuSection, MenuItemModel> GetRootMenuItems()
    {
        var menuItems = new Dictionary<MenuSection, MenuItemModel>();

        menuItems[MenuSection.Dashboard] = new MenuItemModel()
        {
            Name = MenuSection.Dashboard.ToString(),
            IconClass = "fa fa-home",
            ComponentType = typeof(Dashboard),
            SubItems = null
        };

        menuItems[MenuSection.Resources] = new MenuItemModel()
        {
            Name = MenuSection.Resources.ToString(),
            ComponentType = null,
            IconClass = "fa fa-table",
            SubItems = new List<MenuItemModel>(),
        };

        menuItems[MenuSection.Charts] = new MenuItemModel()
        {
            Name = MenuSection.Charts.ToString(),
            ComponentType = null,
            IconClass = "fa fa-bar-chart",
            SubItems = new List<MenuItemModel>(),
        };

        menuItems[MenuSection.Stats] = new MenuItemModel()
        {
            Name = MenuSection.Stats.ToString(),
            ComponentType = null,
            IconClass = "far fa-calendar-alt",
            SubItems = new List<MenuItemModel>(),
        };

        return menuItems;
    }
}