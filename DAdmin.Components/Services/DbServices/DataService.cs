using System.Linq.Expressions;
using System.Reflection;
using Blazorise;
using Blazorise.DataGrid;
using Blazorise.Utilities;
using DAdmin.Components.Extensions;
using DAdmin.Components.Services.DbServices.Interfaces;
using DAdmin.Shared.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DAdmin.Components.Services.DbServices;

//TODO DRY and KISS Violation FIX
public class DataService<TEntity> : DbService, IDataService<TEntity> where TEntity : class
{
    private const int PageSize = 10;

    public DataService(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public async Task<PaginatedResponse<TEntity>> GetPaginatedAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryLogic,
        int page,
        string searchTerm = null
    )
    {
        IQueryable<TEntity> query = Query();

        // Apply custom query logic if provided
        if (queryLogic != null)
        {
            query = queryLogic(query);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var predicate = GetSearchPredicate(searchTerm);
            query = query.Where(predicate);
        }

        int skipAmount = (page - 1) * PageSize;
        var data = await query.Skip(skipAmount).Take(PageSize).ToListAsync();

        var tableEntities = data.Select(item =>
            new TableResource<TEntity>
            {
                EntityModel = item,
                Properties = item.GetType().GetProperties().Select(prop =>
                    new ResourceProperty
                    {
                        TablePropertyInfo = prop,
                        Name = prop.Name,
                        Value = prop.GetValue(item),
                        IsNavigationProperty = DbContext.IsNavigationProperty(item.GetType(), prop),
                    }
                ).ToList()
            }
        ).ToList();

        int totalItems = await query.CountAsync();
        int totalPages = (totalItems + PageSize - 1) / PageSize;

        return new PaginatedResponse<TEntity>
        {
            Data = tableEntities,
            TotalPages = totalPages,
            TotalCount = totalItems
        };
    }

    public async Task<PaginatedResponse<TEntity>> GetPaginatedAsync(
        Func<IQueryable<TEntity>, IQueryable<TEntity>> queryLogic,
        int page,
        IEnumerable<DataGridColumnInfo> columnInfos
    )
    {
        IQueryable<TEntity> query = Query();

        // Apply custom query logic if provided
        if (queryLogic != null)
        {
            query = queryLogic(query);
        }


        foreach (var columnInfo in columnInfos.Where(c =>
                     c.SearchValue != null && !string.IsNullOrEmpty(c.SearchValue.ToString())))
        {
            var propertyInfo = typeof(TEntity).GetProperty(columnInfo.Field);
            if (propertyInfo == null)
            {
                // Skip if the property does not exist
                continue;
            }

            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var property = Expression.Property(parameter, columnInfo.Field);

            Expression predicate;
            var searchValueAsString = columnInfo.SearchValue.ToString();

            if (propertyInfo.PropertyType == typeof(string))
            {
                // Use Contains for string properties
                predicate = Expression.Call(property, nameof(string.Contains), null,
                    Expression.Constant(searchValueAsString));
            }
            else if (propertyInfo.PropertyType.IsNumeric())
            {
                // For numeric properties, convert them to string within the query
                var toStringCall = Expression.Call(property, nameof(object.ToString), Type.EmptyTypes);
                predicate = Expression.Call(toStringCall, nameof(string.Contains), null,
                    Expression.Constant(searchValueAsString));
            }
            else if (propertyInfo.PropertyType == typeof(DateTime) || propertyInfo.PropertyType == typeof(DateTime?))
            {
                // For DateTime properties, convert them to string within the query
                // Adjust the format as necessary
                var toStringCall = Expression.Call(property, nameof(DateTime.ToString), null,
                    Expression.Constant("yyyy-MM-dd"));
                predicate = Expression.Call(toStringCall, nameof(string.Contains), null,
                    Expression.Constant(searchValueAsString));
            }
            else
            {
                // Skip other types
                continue;
            }

            var lambda = Expression.Lambda<Func<TEntity, bool>>(predicate, parameter);
            query = query.Where(lambda);
        }


        // Sorting logic
        var sortedColumnInfos = columnInfos
            .Where(c => !string.IsNullOrWhiteSpace(c.SortField))
            .OrderBy(c => c.SortIndex);
        foreach (var columnInfo in sortedColumnInfos)
        {
            var propertyInfo = typeof(TEntity).GetProperty(columnInfo.SortField);
            if (propertyInfo == null)
            {
                // Handle the case where the sort property does not exist on TEntity
                continue;
            }

            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var property = Expression.Property(parameter, columnInfo.SortField);
            var lambda = Expression.Lambda(property, parameter);

            var methodName = (columnInfo.SortDirection == SortDirection.Ascending ? "OrderBy" : "OrderByDescending");
            var method = typeof(Queryable).GetMethods()
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TEntity), propertyInfo.PropertyType);

            query = (IQueryable<TEntity>)method.Invoke(null, new object[] { query, lambda });
        }

        int skipAmount = (page - 1) * PageSize;
        var data = await query.Skip(skipAmount).Take(PageSize).ToListAsync();

        var tableEntities = data.Select(item =>
            new TableResource<TEntity>
            {
                EntityModel = item,
                Properties = item.GetType().GetProperties().Select(prop =>
                    new ResourceProperty
                    {
                        TablePropertyInfo = prop,
                        Name = prop.Name,
                        Value = prop.GetValue(item),
                        IsNavigationProperty = DbContext.IsNavigationProperty(item.GetType(), prop),
                    }
                ).ToList()
            }
        ).ToList();

        int totalItems = await query.CountAsync();

        int totalPages = (totalItems + PageSize - 1) / PageSize;
        return new PaginatedResponse<TEntity>
        {
            Data = tableEntities,
            TotalPages = totalPages,
            TotalCount = totalItems
        };
    }

    private Expression<Func<TEntity, bool>> GetSearchPredicate(string searchTerm)
    {
        var properties = typeof(TEntity).GetProperties()
            .Where(prop => prop.PropertyType == typeof(string)).ToList();

        var parameter = Expression.Parameter(typeof(TEntity), "item");
        Expression body = Expression.Constant(false);

        foreach (var prop in properties)
        {
            var propAccess = Expression.PropertyOrField(parameter, prop.Name);
            var condition = Expression.Call(
                propAccess,
                typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                Expression.Constant(searchTerm, typeof(string))
            );
            body = Expression.OrElse(body, condition);
        }

        return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
    }

    public async Task<TEntity> CreateAsync(string tableName, TEntity entity)
    {
        var entityType = DbContext.Model.GetEntityTypes()
            .FirstOrDefault(e => e.ClrType.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));

        if (entityType == null) return default;

        var method = typeof(DbContext).GetMethod(nameof(DbContext.Set),
                BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null)
            .MakeGenericMethod(entityType.ClrType);

        var dbSet = method.Invoke(DbContext, null) as DbSet<TEntity>;

        var idProperty = entityType.ClrType.GetProperty("Id");
        if (idProperty != null && entity != null)
        {
            var idValue = idProperty.GetValue(entity);
            if (idProperty.PropertyType == typeof(Guid) && (Guid)idValue == Guid.Empty)
            {
                idProperty.SetValue(entity, Guid.NewGuid());
            }
        }

        var result = await dbSet.AddAsync(entity);
        await DbContext.SaveChangesAsync();

        return result.Entity;
    }

    public async Task<object> CreateAsync(string entityName, object entity)
    {
        var entityType = DbContext.Model.GetEntityTypes()
            .FirstOrDefault(e => e.ClrType.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase));

        if (entityType == null) return null;

        // Get the generic method for DbSet<T>.
        var dbSetMethod = typeof(DbContext).GetMethods()
            .First(m => m.IsGenericMethod && m.Name == nameof(DbContext.Set))
            .MakeGenericMethod(entityType.ClrType);

        // Invoke the method to get the DbSet for the specific entity type.
        var dbSet = dbSetMethod.Invoke(DbContext, null);

        PropertyInfo idProperty = entityType.FindPrimaryKey().Properties
            .FirstOrDefault()?.PropertyInfo;

        if (idProperty?.PropertyType == typeof(Guid))
        {
            var idValue = idProperty.GetValue(entity);
            if ((Guid)idValue == Guid.Empty)
            {
                idProperty.SetValue(entity, Guid.NewGuid());
            }
        }

        // Get the Add method for the DbSet<T> appropriate to the entity type.
        var addMethod = dbSet.GetType().GetMethods()
            .First(m => m.Name == "Add" && m.GetParameters().Length == 1 &&
                        m.GetParameters()[0].ParameterType.IsAssignableFrom(entityType.ClrType));

        // Use MethodInfo.Invoke to add the entity to the DbSet.
        var entityEntry = addMethod.Invoke(dbSet, new[] { entity }) as EntityEntry;

        await DbContext.SaveChangesAsync();

        return entityEntry?.Entity;
    }

    public IQueryable<TEntity> Query()
    {
        return DbContext.Set<TEntity>().AsQueryable();
    }

    public async Task<TEntity> UpdateAsync(string tableName, TEntity entity)
    {
        try
        {
            DbContext.Entry(entity).State = EntityState.Modified;
            await DbContext.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            // Consider logging the entire exception, including the stack trace
            Console.WriteLine(ex.ToString());
            throw;
        }
    }

    public async Task DeleteAsync(string tableName, TEntity entity)
    {
        try
        {
            DbContext.Remove(entity);
            await DbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message != null)
            {
                throw new DbUpdateException(ex.InnerException?.Message);
            }
            else
            {
                throw new DbUpdateException(ex.Message);
            }
        }
    }

    private static LambdaExpression CreateEqualsExpression(Type entityType, string propertyName, object value)
    {
        var param = Expression.Parameter(entityType);
        var prop = Expression.Property(param, propertyName);
        var constValue = Expression.Constant(value);
        var equals = Expression.Equal(prop, constValue);
        return Expression.Lambda(equals, param);
    }
}