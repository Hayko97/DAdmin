using System.Reflection;
using DynamicAdmin.Components.Builders;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DynamicAdmin.Components.Services.Interfaces;

public interface IDbInfoService
{
    IEnumerable<IEntityType> GetEntityTypes();
    IEnumerable<string> GetEntityNames();
    StatsBuilder GetStatsBuilder();
    Task<string> GetEntityName(PropertyInfo propertyInfo);
    Task<Type> GetEntityType(string entityName);
}