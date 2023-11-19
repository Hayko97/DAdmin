using System.Reflection;
using DAdmin.Builders;
using DAdmin.Services.DbServices.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DAdmin.Services.DbServices;

public class DbInfoService : DbService, IDbInfoService
{
    public DbInfoService(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public IEnumerable<IEntityType> GetEntityTypes()
    {
        return DbContext.Model.GetEntityTypes().ToList();
    }

    public IEnumerable<string> GetEntityNames()
    {
        return DbContext.Model.GetEntityTypes().Select(x => x.ClrType.Name).ToList();
    }

    public StatsBuilder GetStatsBuilder()
    {
        return new StatsBuilder(DbContext);
    }

    public Task<string> GetEntityName(PropertyInfo propertyInfo)
    {
        var entityType = propertyInfo.DeclaringType;
        if (entityType == null)
        {
            throw new ArgumentException("The property does not have a declaring type.");
        }

        var efEntityType = DbContext.Model.FindEntityType(entityType);
        if (efEntityType == null)
        {
            throw new ArgumentException("The type is not an entity in the current DbContext.");
        }

        // Use the IRelationalEntityTypeExtensions.GetEntityName() extension method to retrieve the table name
        var tableName = efEntityType.ClrType.Name;

        if (string.IsNullOrEmpty(tableName))
        {
            throw new InvalidOperationException(
                $"The entity type '{entityType.Name}' does not have an associated table.");
        }

        return Task.FromResult(tableName);
    }

    public Task<Type> GetEntityType(string entityName)
    {
        var entityType = DbContext.Model.GetEntityTypes()
            .FirstOrDefault(t => t.ClrType.Name == entityName)?.ClrType;

        if (entityType == null) throw new ArgumentException("Invalid entity name");

        return Task.FromResult(entityType);
    }
}