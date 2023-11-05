using System.Collections;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace DynamicAdmin.Components.Helpers;

public static class DbContextHelper
{
    public static Type GetTypeFromTableName(this DbContext context, string tableName)
    {
        return context?.Model.GetEntityTypes()
            .FirstOrDefault(e => e.ClrType.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase))
            ?.ClrType;
    }

    public static IEnumerable<PropertyInfo> GetPropertiesWithoutRelations(this DbContext context, Type type)
    {
        var model = context.Model;
        var entity = model.FindEntityType(type);

        // Get navigation properties that are mapped to collection types
        var collectionNavigationProperties = new HashSet<string>(
            entity.GetNavigations()
                .Where(np => IsCollectionType(np.ClrType))
                .Select(np => np.Name));

        // Exclude collection navigation properties
        return type.GetProperties()
            .Where(p => !collectionNavigationProperties.Contains(p.Name))
            .ToList();
    }

    private static bool IsCollectionType(Type type)
    {
        return typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string);
    }

    public static bool IsNavigationProperty(this DbContext dbContext, Type data, PropertyInfo prop)
    {
        var entityType = dbContext.Model.FindEntityType(data);

        if (entityType == null) return false;

        // Check if it's a recognized navigation property
        var navigation = entityType.FindNavigation(prop.Name);

        if (prop.Name.EndsWith("Id") &&
            entityType.GetNavigations().Any(nav => nav.Name == prop.Name.Substring(0, prop.Name.Length - 2)))
        {
            return false;
        }

        // It's a navigation property if EF Core recognizes it as such
        return navigation != null;
    }

    public static bool IsForeignKey(this DbContext dbContext, Type data, PropertyInfo prop)
    {
        var entityType = dbContext.Model.FindEntityType(data);
        if (entityType == null) return false;

        if (prop.Name.EndsWith("Id") &&
            entityType.GetNavigations().Any(nav => nav.Name == prop.Name.Substring(0, prop.Name.Length - 2)))
        {
            return true;
        }

        return false;
    }
    
    public static PropertyInfo GetForeignKeyForNavigationProperty(this DbContext dbContext, Type entityType, PropertyInfo navigationProperty)
    {
        var foreignKey = dbContext.Model.FindEntityType(entityType)
            .FindNavigation(navigationProperty.Name)?.ForeignKey;

        return foreignKey?.Properties.FirstOrDefault()?.PropertyInfo;
    }

    public static IEnumerable<object> GetRelatedEntitiesFor(this DbContext dbContext, PropertyInfo prop)
    {
        Type typeToQuery;

        // If the property is a collection (like ICollection<Transaction>), get the underlying type (Transaction in this case)
        if (prop.PropertyType.IsGenericType &&
            prop.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
        {
            typeToQuery = prop.PropertyType.GetGenericArguments()[0];
        }
        else
        {
            typeToQuery = prop.PropertyType;
        }

        var method = dbContext.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.IsGenericMethod
                        && m.Name == nameof(DbContext.Set)
                        && m.GetParameters().Length == 0)
            .Single()
            .MakeGenericMethod(typeToQuery);

        return method.Invoke(dbContext, null) as IEnumerable<object>;
    }

    public static object GetPropertyValue(object obj, string propName)
    {
        return obj.GetType().GetProperty(propName)?.GetValue(obj);
    }
}