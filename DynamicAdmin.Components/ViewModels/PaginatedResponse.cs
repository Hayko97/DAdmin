namespace DynamicAdmin.Components.ViewModels;

public class PaginatedResponse<TEntity> where TEntity : class
{
    public List<EntityViewModel<TEntity>> Data { get; set; }
    public int TotalPages { get; set; }
}
