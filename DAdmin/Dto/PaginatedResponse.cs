namespace DAdmin.Dto;

public class PaginatedResponse<TEntity> where TEntity : class
{
    public List<DataResourceDto<TEntity>> Data { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
}
