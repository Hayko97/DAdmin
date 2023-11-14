namespace DAdmin.Shared.DTO;

public class PaginatedResponse<TEntity> where TEntity : class
{
    public List<DataResource<TEntity>> Data { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
}
