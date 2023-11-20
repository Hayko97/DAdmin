using System.Reflection;

namespace DAdmin.Dto;

public record DataProperty
{
    public PropertyInfo EntityPropertyInfo { get; set; }
    public bool IsKey { get; set; }
    public string Name { get; set; }
    public object Value { get; set; }
    public bool IsDefaultValue { get; set; }
    public bool IsForeignKey { get; set; }
    public List<KeyValuePair<string, string>>? RelatedEntities { get; set; } //name, jsonString //TODO refactor
    public bool IsNavigationProperty { get; set; } // ICollection<DataResourceDto>
    public PropertyInfo? ForeignKeyProperty { get; set; } // Associated foreignKey if has relation
}