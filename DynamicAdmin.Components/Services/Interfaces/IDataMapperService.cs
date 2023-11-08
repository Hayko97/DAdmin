using DynamicAdmin.Components.ViewModels;

namespace DynamicAdmin.Components.Services.Interfaces;

public interface IDataMapperService<TEntity> where TEntity : class
{
    Task<EntityViewModel<TEntity>> GetEntityViewModel(TEntity entity);

    Task<EntityViewModel<object>> GetEntityViewModel(object entity);
}