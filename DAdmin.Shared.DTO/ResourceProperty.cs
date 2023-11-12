using System.Reflection;

namespace DAdmin.Shared.DTO;

public record ResourceProperty
{
    public PropertyInfo TablePropertyInfo { get; set; }
    public bool IsKey { get; set; }
    public string Name { get; set; }
    public object Value { get; set; }
    public bool IsDefaultValue { get; set; }
    public bool IsForeignKey { get; set; }
    public List<KeyValuePair<string, string>>? RelatedEntities { get; set; }
    public bool IsNavigationProperty { get; set; } // ICollection<TableResource>
    public PropertyInfo? ForeignKeyProperty { get; set; } // Associated foreignKey
}