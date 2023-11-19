using DAdmin.Menus.ViewModels;

namespace DAdmin.Services;

public interface IMenuService
{
    Task<Dictionary<MenuSection, MenuItemModel>> AddDefaultMenuItems(Dictionary<MenuSection, MenuItemModel> menuItems);
    Task<Dictionary<MenuSection, MenuItemModel>> AddEntitiesToResources(Dictionary<MenuSection, MenuItemModel> menuItems);

    Dictionary<MenuSection, MenuItemModel> GetRootMenuItems();
}