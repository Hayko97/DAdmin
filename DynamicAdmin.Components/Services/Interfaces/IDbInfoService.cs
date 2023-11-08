using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DynamicAdmin.Components.Services.Interfaces;

public interface IDbInfoService
{
    IEnumerable<IEntityType> GetEntityTypes();
    IEnumerable<string> GetEntityNames();

    Task<string> GetEntityName(PropertyInfo propertyInfo);
    Task<Type> GetEntityType(string entityName);
}