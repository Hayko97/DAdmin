using System.Reflection;
using DynamicAdmin.Components.Helpers;
using DynamicAdmin.Components.Services.Interfaces;
using DynamicAdmin.Components.ViewModels;

namespace DynamicAdmin.Components.Services;

public class DataMapperService<TEntity> : DbService, IDataMapperService<TEntity> where TEntity : class
{
    public DataMapperService(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
    
    public async Task<EntityViewModel<TEntity>> GetEntityViewModel(TEntity entity)
    {
        return await GetEntityViewModelInternal(entity, typeof(TEntity));
    }

    public async Task<EntityViewModel<object>> GetEntityViewModel(object entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var entityType = entity.GetType();
    
        // Check if the type is a valid entity type, assuming this is a method you'd implement
        if (!DbContext.IsValidEntityType(entityType))
        {
            throw new ArgumentException("The provided object is not a valid entity type.", nameof(entity));
        }

        return await GetEntityViewModelInternal(entity, entityType);
    }


    private Task<EntityViewModel<T>> GetEntityViewModelInternal<T>(T entity, Type entityType)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
        
        var properties = entityType.GetProperties();

        var model = new EntityViewModel<T>
        {
            Entity = entity,
            Properties = properties.Select(prop => CreateEntityProperty(prop, entity, properties)).ToList()
        };

        return Task.FromResult(model);
    }
    
    private bool IsNavigationProperty(Type entityType, PropertyInfo propertyInfo)
    {
        return DbContext.IsNavigationProperty(entityType, propertyInfo);
    }

    private List<KeyValuePair<string, string>> GetRelatedEntities(PropertyInfo navigationProperty)
    {
        return DbContext.GetRelatedEntitiesFor(navigationProperty)
            .Select(re => new KeyValuePair<string, string>(
                DbContextHelper.GetPropertyValue(re, "Id").ToString(),
                string.Join(", ", re.GetType().GetProperties().Skip(1).Take(3).Select(p => p.GetValue(re)?.ToString() ?? string.Empty))
            )).ToList();
    }

    private EntityProperty CreateEntityProperty(PropertyInfo prop, object entity, PropertyInfo[] properties)
    {
        bool isNavigation = IsNavigationProperty(entity.GetType(), prop);
        var relatedEntities = isNavigation ? GetRelatedEntities(prop) : null;
        var foreignKeyProperty = DbContext.GetForeignKeyForNavigationProperty(entity.GetType(), prop);

        var entityProperty = new EntityProperty
        {
            TablePropertyInfo = prop,
            Name = prop.Name,
            Value = prop.GetValue(entity),
            IsNavigationProperty = isNavigation,
            RelatedEntities = relatedEntities,
            IsForeignKey = DbContext.IsForeignKey(entity.GetType(), prop),
            ForeignKeyProperty = foreignKeyProperty != null && properties.Any(x => x.Name == foreignKeyProperty.Name) ? foreignKeyProperty : null
        };

        return entityProperty;
    }
    
}