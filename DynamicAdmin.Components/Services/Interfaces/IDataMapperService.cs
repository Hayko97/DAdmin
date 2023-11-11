using DynamicAdmin.Components.ViewModels;

namespace DynamicAdmin.Components.Services.Interfaces;

public interface IDataMapperService<TEntity> where TEntity : class
{
    Task<Entity<TEntity>> GetEntityViewModel(TEntity entity);

    Task<Entity<object>> GetEntityViewModel(object entity);
}