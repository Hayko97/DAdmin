using System.Reflection;
using DAdmin.Builders;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DAdmin.Services.DbServices.Interfaces;

public interface IDbInfoService
{
    IEnumerable<IEntityType> GetEntityTypes();
    IEnumerable<string> GetEntityNames();
    StatsBuilder GetStatsBuilder();
    Task<string> GetEntityName(PropertyInfo propertyInfo);
    Task<Type> GetEntityType(string entityName);
}