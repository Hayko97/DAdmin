using DAdmin.Shared.DTO;

namespace DAdmin.Services.DbServices.Interfaces;

public interface IDataMapperService<TEntity> where TEntity : class
{
    Task<DataResourceDto<TEntity>> MapToTableResource(TEntity entity);

    Task<DataResourceDto<object>> MapToTableResource(object entity);
}