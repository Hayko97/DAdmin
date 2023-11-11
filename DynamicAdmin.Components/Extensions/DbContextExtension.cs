using System.Collections;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace DynamicAdmin.Components.Extensions;

public static class DbContextExtension
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

        // Check if the property is a foreign key
        var foreignKeys = entityType.GetForeignKeys();
        foreach (var fk in foreignKeys)
        {
            if (fk.Properties.Any(p => p.Name == prop.Name))
            {
                return true;
            }
        }

        return false;
    }

    public static PropertyInfo GetForeignKeyForNavigationProperty(this DbContext dbContext, Type entityType,
        PropertyInfo navigationProperty)
    {
        var foreignKey = dbContext.Model.FindEntityType(entityType)
            .FindNavigation(navigationProperty.Name)?.ForeignKey;

        return foreignKey?.Properties.FirstOrDefault()?.PropertyInfo;
    }
    
    public static bool IsKey(this DbContext dbContext, Type entityType, PropertyInfo property)
    {
        var modelEntityType = dbContext.Model.FindEntityType(entityType);
        if (modelEntityType == null) return false;

        // Check if the property is a part of the primary key
        var primaryKey = modelEntityType.FindPrimaryKey();
        if (primaryKey != null && primaryKey.Properties.Any(p => p.Name == property.Name))
        {
            return true;
        }

        return false;
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

    public static bool IsValidEntityType(this DbContext dbContext, Type entityType)
    {
        var model = dbContext.Model;
        return model.FindEntityType(entityType) != null;
    }

    public static string GetTableNameFromProperty(this DbContext dbContext, PropertyInfo propertyInfo)
    {
        if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
        if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));

        var entityType = propertyInfo.DeclaringType;
        if (entityType == null)
        {
            throw new ArgumentException("The property does not have a declaring type.");
        }

        var efEntityType = dbContext.Model.FindEntityType(entityType);
        if (efEntityType == null)
        {
            throw new ArgumentException("The type is not an entity in the current DbContext.");
        }

        // Use the IRelationalEntityTypeExtensions.GetEntityName() extension method to retrieve the table name
        var tableName = efEntityType.ClrType.Name;
    
        if (string.IsNullOrEmpty(tableName))
        {
            throw new InvalidOperationException($"The entity type '{entityType.Name}' does not have an associated table.");
        }

        return tableName;
    }

}