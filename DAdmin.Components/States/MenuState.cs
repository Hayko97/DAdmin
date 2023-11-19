using System.ComponentModel;
using DAdmin.Menus.ViewModels;

namespace DAdmin.Components.States;

public class MenuState : INotifyPropertyChanged
{
    private Dictionary<MenuSection, MenuItemModel> _menuItems;

    public Dictionary<MenuSection, MenuItemModel> MenuItems
    {
        get => _menuItems;
        set
        {
            _menuItems = value;
            OnPropertyChanged(nameof(MenuItems));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public Task AddMenuItemAsync(MenuItemModel itemModel)
    {
        if (MenuItems[itemModel.Section].SubItems == null)
        {
            MenuItems[itemModel.Section].SubItems = new List<MenuItemModel>();
        }

        MenuItems[itemModel.Section].SubItems?.Add(itemModel);
        OnPropertyChanged(nameof(MenuItems));

        return Task.CompletedTask;
    }
}