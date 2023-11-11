using System.Reflection;

namespace DynamicAdmin.Components.ViewModels;

public record EntityProperty
{
    public PropertyInfo TablePropertyInfo { get; set; }
    public bool IsKey { get; set; }
    public string Name { get; set; }
    public object Value { get; set; }
    public bool IsDefaultValue { get; set; }
    public bool IsForeignKey { get; set; }
    public List<KeyValuePair<string, string>>? RelatedEntities { get; set; }
    public bool IsNavigationProperty { get; set; } // ICollection<Entity>
    public PropertyInfo? ForeignKeyProperty { get; set; } // Associated foreignKey
}