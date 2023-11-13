using DAdmin.Components.Components.Menus.ViewModels;

namespace DAdmin.Components.Services;

public interface IMenuService
{
    Task<Dictionary<MenuType, MenuItem>> GetDefaultMenu();

    Dictionary<MenuType, MenuItem> GetRootMenuItems();
}