using DAdmin.Components.Components.Menus.ViewModels;

namespace DAdmin.Components.Services;

public interface IMenuService
{
    Task<Dictionary<MenuSection, MenuItemModel>> AddDefaultMenuItems(Dictionary<MenuSection, MenuItemModel> menuItems);
    Task<Dictionary<MenuSection, MenuItemModel>> AddEntitiesToResources(Dictionary<MenuSection, MenuItemModel> menuItems);

    Dictionary<MenuSection, MenuItemModel> GetRootMenuItems();
}