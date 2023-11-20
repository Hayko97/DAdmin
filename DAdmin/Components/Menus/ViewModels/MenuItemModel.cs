using Microsoft.AspNetCore.Components;

namespace DAdmin.Menus.ViewModels;

public record MenuItemModel
{
    public string Name { get; set; }
    public string IconClass { get; set; }
    public MenuSection Section { get; set; }
    public Type? ComponentType { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public List<MenuItemModel>? SubItems { get; set; }
    public RenderFragment Content { get; set; }
    
    public RenderFragment RenderContent() => builder =>
    {
        if (ComponentType != null)
        {
            builder.OpenComponent(0, ComponentType);
            if (Parameters != null)
            {
                foreach (var parameter in Parameters)
                {
                    builder.AddAttribute(1, parameter.Key, parameter.Value);
                }
            }

            builder.CloseComponent();
        }
    };
}