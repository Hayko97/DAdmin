namespace DynamicAdmin.Components.ViewModels;

public record EntityViewModel<TEntity> where TEntity : class
{
    public TEntity Entity { get; set; }
    public IEnumerable<EntityProperty> Properties { get; set; }

    public IEnumerable<EntityProperty> GetPropertiesWithoutRelations()
    {
        return Properties.Where(x => !x.IsNavigationProperty).ToList();
    }
}