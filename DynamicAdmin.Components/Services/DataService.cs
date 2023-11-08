using System.Reflection;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using DynamicAdmin.Components.Helpers;
using DynamicAdmin.Components.Services.Interfaces;
using DynamicAdmin.Components.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;

namespace DynamicAdmin.Components.Services;

public class DataService<TEntity> : DbService, IDataService<TEntity> where TEntity : class
{
    private const int PageSize = 10;
    
    public DataService(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public async Task<PaginatedResponse<TEntity>> GetPaginatedAsync(string tableName, int page,
        string searchTerm = null)
    {
        // 
        var entityType = DbContext.Model.GetEntityTypes()
            .FirstOrDefault(e => e.ClrType.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));

        if (entityType == null) return null;

        var method = typeof(DbContext).GetMethod(nameof(DbContext.Set),
                BindingFlags.Public | BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null)
            .MakeGenericMethod(entityType.ClrType);

        var dbSet = method.Invoke(DbContext, null) as IQueryable<TEntity>;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var predicate = GetSearchPredicate(searchTerm);
            dbSet = dbSet.Where(predicate);
        }

        int skipAmount = (page - 1) * PageSize;
        var data = await dbSet.Skip(skipAmount).Take(PageSize).ToListAsync();

        var tableEntities = data.Select(item =>
            new EntityViewModel<TEntity>
            {
                Entity = item,
                Properties = item.GetType().GetProperties().Select(prop =>
                    new EntityProperty
                    {
                        TablePropertyInfo = prop,
                        Name = prop.Name,
                        Value = prop.GetValue(item),
                        IsNavigationProperty = DbContext.IsNavigationProperty(item.GetType(), prop),
                    }
                ).ToList()
            }
        ).ToList();

        int totalItems = await dbSet.CountAsync();
        int totalPages = (totalItems + PageSize - 1) / PageSize;

        return new PaginatedResponse<TEntity>
        {
            Data = tableEntities,
            TotalPages = totalPages
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
        catch (DbUpdateException e)
        {
            throw new Exception("The DELETE statement conflicted with the REFERENCE constraint");
        }
    }

    public async Task DeleteWithReferencesAsync(string tableName, TEntity entity)
    {
        // 1. Identify and load referencing records
        var references = DbContext.Model.FindEntityType(typeof(TEntity))
            .GetReferencingForeignKeys();

        foreach (var fk in references)
        {
            var relatedEntityType = fk.DeclaringEntityType.ClrType;
            var setMethod = typeof(DbContext)
                .GetMethod(nameof(DbContext.Set), BindingFlags.Public | BindingFlags.Instance)
                .MakeGenericMethod(relatedEntityType);
            var set = setMethod.Invoke(DbContext, null);

            var foreignKeyPropertyName = fk.Properties.First().Name;
            var keyValue = entity.GetType().GetProperty("Id").GetValue(entity);

            var whereMethod = typeof(Queryable).GetMethods()
                .First(m => m.Name == "Where" && m.GetParameters().Length == 2)
                .MakeGenericMethod(relatedEntityType);
            var lambda = CreateEqualsExpression(relatedEntityType, foreignKeyPropertyName, keyValue);
            var recordsToDelete = whereMethod.Invoke(null, new object[] { set, lambda }) as IQueryable;

            DbContext.RemoveRange(recordsToDelete);
        }

        // 2. Delete the main record
        DbContext.Remove(entity);

        // 3. Save the changes
        await DbContext.SaveChangesAsync();
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