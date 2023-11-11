namespace DynamicAdmin.Components.ViewModels;

public record Entity<TEntity>
{
    public TEntity EntityModel { get; set; }
    public string Name { get; set; }
    public IEnumerable<EntityProperty> Properties { get; set; }

    public IEnumerable<EntityProperty> GetPropertiesWithoutRelations()
    {
        return Properties.Where(x => !x.IsNavigationProperty && !x.IsKey).ToList();
    }
}