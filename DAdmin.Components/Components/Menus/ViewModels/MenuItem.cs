using Microsoft.AspNetCore.Components;

namespace DAdmin.Components.Components.Menus.ViewModels;

public class MenuItem
{
    public string Name { get; set; }
    public MenuType Type { get; set; }

    public Type ComponentType { get; set; }

    public List<MenuItem>? SubItems { get; set; }

    public Dictionary<string, object> Parameters { get; set; } = new();

    private RenderFragment RenderContent() => builder =>
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