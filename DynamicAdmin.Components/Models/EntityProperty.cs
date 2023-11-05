using System.Reflection;

namespace DynamicAdmin.Components.Models;

public record EntityProperty
{
    public PropertyInfo TablePropertyInfo { get; set; }
    public string Name { get; set; }
    public object Value { get; set; }
    public bool IsForeignKey { get; set; }
    public IEnumerable<KeyValuePair<string, string>>? RelatedEntities { get; set; }
    
    public bool IsNavigationProperty { get; set; } // ICollection<Entity>
    public PropertyInfo? ForeignKeyProperty { get; set; } // Associated foreignKey
}
