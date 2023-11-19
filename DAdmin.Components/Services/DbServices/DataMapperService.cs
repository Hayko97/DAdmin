using System.Reflection;
using DAdmin.Components.Extensions;
using DAdmin.Components.Helpers;
using DAdmin.Components.Services.DbServices.Interfaces;
using DAdmin.Shared.DTO;

namespace DAdmin.Components.Services.DbServices;

public class DataMapperService<TEntity> : DbService, IDataMapperService<TEntity> where TEntity : class
{
    public DataMapperService(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public async Task<DataResourceDto<TEntity>> MapToTableResource(TEntity entity)
    {
        return await GetEntityViewModelInternal(entity);
    }

    public async Task<DataResourceDto<object>?> MapToTableResource(object entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var entityType = entity.GetType();

        // Check if the type is a valid entity type, assuming this is a method you'd implement
        if (!DbContext.IsValidEntityType(entityType))
        {
            throw new ArgumentException("The provided object is not a valid entity type.", nameof(entity));
        }

        return await GetEntityViewModelInternal(entity);
    }


    private Task<DataResourceDto<T>> GetEntityViewModelInternal<T>(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var properties = entity.GetType().GetProperties();

        var model = new DataResourceDto<T>
        {
            Name = entity.GetType().Name,
            EntityModel = entity,
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
                ClassHelper.GetPropertyValue(re, "Id").ToString(),
                string.Join(", ",
                    re.GetType().GetProperties()
                        .Skip(2)
                        .Take(5)
                        .Select(p => p.GetValue(re)?.ToString() ?? string.Empty))
            )).ToList();
    }

    private DataProperty CreateEntityProperty(PropertyInfo prop, object entity, PropertyInfo[] properties)
    {
        bool isNavigation = IsNavigationProperty(entity.GetType(), prop);
        var relatedEntities = isNavigation ? GetRelatedEntities(prop) : null;
        var foreignKeyProperty = DbContext.GetForeignKeyForNavigationProperty(entity.GetType(), prop);
        object propertyValue = prop.GetValue(entity);
        bool isNullOrDefault = ClassHelper.IsNullOrDefaultValue(prop, propertyValue);

        var entityProperty = new DataProperty
        {
            EntityPropertyInfo = prop,
            Name = prop.Name,
            IsKey = DbContext.IsKey(entity.GetType(), prop),
            Value = prop.GetValue(entity),
            IsNavigationProperty = isNavigation,
            RelatedEntities = relatedEntities,
            IsForeignKey = DbContext.IsForeignKey(entity.GetType(), prop),
            ForeignKeyProperty = foreignKeyProperty != null && properties.Any(x => x.Name == foreignKeyProperty.Name)
                ? foreignKeyProperty
                : null,
            IsDefaultValue = isNullOrDefault
        };

        return entityProperty;
    }
}