namespace DynamicAdmin.Components.Models;

public class PaginatedResponse<TEntity>
{
    public List<EntityViewModel<TEntity>> Data { get; set; }
    public int TotalPages { get; set; }
}
