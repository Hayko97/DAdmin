namespace DynamicAdmin.Components.ViewModels;

public class PaginatedResponse<TEntity> where TEntity : class
{
    public List<Entity<TEntity>> Data { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
}
