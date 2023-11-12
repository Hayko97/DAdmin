using System.Reflection;
using DAdmin.Components.Builders;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DAdmin.Components.Services.Interfaces;

public interface IDbInfoService
{
    IEnumerable<IEntityType> GetEntityTypes();
    IEnumerable<string> GetEntityNames();
    StatsBuilder GetStatsBuilder();
    Task<string> GetEntityName(PropertyInfo propertyInfo);
    Task<Type> GetEntityType(string entityName);
}