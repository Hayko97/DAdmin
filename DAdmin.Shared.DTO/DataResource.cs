namespace DAdmin.Shared.DTO;

public record DataResource<TEntity>
{
    public TEntity EntityModel { get; set; }
    public string Name { get; set; }
    public IEnumerable<ResourceProperty> Properties { get; set; }

    public IEnumerable<ResourceProperty> GetPropertiesWithoutRelations()
    {
        return Properties.Where(x => !x.IsNavigationProperty && !x.IsKey).ToList();
    }
}