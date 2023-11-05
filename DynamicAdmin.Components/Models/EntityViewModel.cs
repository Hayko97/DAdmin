namespace DynamicAdmin.Components.Models;

public record EntityViewModel<TEntity>
{
    public TEntity Entity { get; set; }
    public IEnumerable<EntityProperty> Properties { get; set; }

    public IEnumerable<EntityProperty> GetPropertiesWithoutRelations()
    {
        return Properties.Where(x => !x.IsNavigationProperty).ToList();
    }
}