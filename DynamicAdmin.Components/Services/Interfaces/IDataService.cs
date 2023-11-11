using DynamicAdmin.Components.ViewModels;

namespace DynamicAdmin.Components.Services.Interfaces;

public interface IDataService<TEntity> where TEntity : class
{
    Task<PaginatedResponse<TEntity>> GetPaginatedAsync(string tableName, int page, string searchTerm = null);

    Task<TEntity> CreateAsync(string tableName, TEntity entity);
    Task<object> CreateAsync(string tableName, object entity);
    Task<TEntity> UpdateAsync(string tableName, TEntity entity);
    Task DeleteAsync(string tableName, TEntity entity);

    IQueryable<TEntity> Query();
}