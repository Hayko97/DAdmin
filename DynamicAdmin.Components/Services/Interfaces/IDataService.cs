using DynamicAdmin.Components.Models;

namespace DynamicAdmin.Components.Services.Interfaces;

public interface IDataService<TEntity>
{
    Task<PaginatedResponse<TEntity>> GetPaginatedAsync(string tableName, int page, string searchTerm = null);

    Task<TEntity> CreateAsync(string tableName, TEntity entity);
    Task<TEntity> UpdateAsync(string tableName, TEntity entity);
    Task DeleteAsync(string tableName, TEntity entity);

    Task<EntityViewModel<TEntity>> GetEntityViewModel(TEntity entity);
}