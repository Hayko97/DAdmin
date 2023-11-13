using DAdmin.Shared.DTO;

namespace DAdmin.Components.Services.DbServices.Interfaces;

public interface IDataMapperService<TEntity> where TEntity : class
{
    Task<TableResource<TEntity>> MapToTableResource(TEntity entity);

    Task<TableResource<object>> MapToTableResource(object entity);
}