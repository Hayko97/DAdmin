using DAdmin.Shared.DTO;

namespace DAdmin.Components.Services.DbServices.Interfaces;

public interface IDataMapperService<TEntity> where TEntity : class
{
    Task<DataResource<TEntity>> MapToTableResource(TEntity entity);

    Task<DataResource<object>> MapToTableResource(object entity);
}