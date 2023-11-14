using DAdmin.Components.Components.Menus.ViewModels;

namespace DAdmin.Components.Services;

public interface IMenuService
{
    Task<Dictionary<MenuType, MenuItem>> AddDefaultMenuItems(Dictionary<MenuType, MenuItem> menuItems);
    Task<Dictionary<MenuType, MenuItem>> AddEntitiesToResources(Dictionary<MenuType, MenuItem> menuItems);

    Dictionary<MenuType, MenuItem> GetRootMenuItems();
}