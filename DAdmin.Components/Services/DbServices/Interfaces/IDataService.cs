using Blazorise.DataGrid;
using DAdmin.Shared.DTO;

namespace DAdmin.Components.Services.DbServices.Interfaces;

public interface IDataService<TEntity> where TEntity : class
{
    Task<PaginatedResponse<TEntity>> GetPaginatedAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryLogic,
        int page,
        string searchTerm = null
    );

    Task<PaginatedResponse<TEntity>> GetPaginatedAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryLogic,
        int page,
        IEnumerable<DataGridColumnInfo> columnInfos
    );

    Task<TEntity> CreateAsync(string tableName, TEntity entity);
    Task<object> CreateAsync(string tableName, object entity);
    Task<TEntity> UpdateAsync(string tableName, TEntity entity);
    Task DeleteAsync(string tableName, TEntity entity);
    IQueryable<TEntity> Query();
}