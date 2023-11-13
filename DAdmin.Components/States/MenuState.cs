using System.ComponentModel;
using DAdmin.Components.Components.Menus.ViewModels;

namespace DAdmin.Components.States;

public class MenuState : INotifyPropertyChanged
{
    private Dictionary<MenuType, MenuItem> _menuItems;

    public Dictionary<MenuType, MenuItem> MenuItems
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

    public Task AddMenuItemAsync(MenuItem item)
    {
        MenuItems.Add(item.Type, item);
        OnPropertyChanged(nameof(MenuItems));

        return Task.CompletedTask;
    }
}