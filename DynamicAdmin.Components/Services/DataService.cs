using System.Reflection;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using DynamicAdmin.Components.Helpers;
using DynamicAdmin.Components.Models;
using Microsoft.VisualBasic;

namespace DynamicAdmin.Components.Services;

public class DataService<TEntity> : IDataService<TEntity>, IDisposable where TEntity : class
{
    private const int PageSize = 10;
    private DbContext _dbContext;

    public DataService(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedResponse<TEntity>> GetPaginatedAsync(string tableName, int page, string searchTerm = null)
    {
        var entityType = _dbContext.Model.GetEntityTypes()
            .FirstOrDefault(e => e.ClrType.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));

        if (entityType == null) return null;

        var method = typeof(DbContext).GetMethod(nameof(DbContext.Set),
                BindingFlags.Public | BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null)
            .MakeGenericMethod(entityType.ClrType);

        var dbSet = method.Invoke(_dbContext, null) as IQueryable<TEntity>;

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
                        IsNavigationProperty = _dbContext.IsNavigationProperty(item.GetType(), prop),
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

    public async Task<EntityViewModel<TEntity>> GetEntityViewModel(TEntity entity)
    {
        var model = new EntityViewModel<TEntity>
        {
            Entity = entity
        };
        var properties = entity.GetType().GetProperties();
        // ReSharper disable once ComplexConditionExpression
        model.Properties = properties.Select(prop =>
        {
            bool isNavigation = _dbContext.IsNavigationProperty(entity.GetType(), prop);
            var relatedEntities = isNavigation
                ? _dbContext.GetRelatedEntitiesFor(prop).Select(re => new KeyValuePair<string, string>(
                    DbContextHelper.GetPropertyValue(re, "Id").ToString(),
                    string.Join(", ", re.GetType().GetProperties().Skip(1).Take(3).Select(s => s.Name))
                )).ToList()
                : null;
            var foreignKeyProperty = _dbContext.GetForeignKeyForNavigationProperty(entity.GetType(), prop);

            var entityProperty = new EntityProperty
            {
                TablePropertyInfo = prop,
                Name = prop.Name,
                Value = prop.GetValue(entity),
                IsNavigationProperty = isNavigation,
                RelatedEntities = relatedEntities,
                IsForeignKey = _dbContext.IsForeignKey(entity.GetType(), prop),
            };

            if (foreignKeyProperty != null && properties.Any(x => x.Name == foreignKeyProperty.Name))
            {
                entityProperty.ForeignKeyProperty = foreignKeyProperty;
            }

            return entityProperty;
        }).ToList();

        return model;
    }


    public async Task<TEntity> CreateAsync(string tableName, TEntity entity)
    {
        var entityType = _dbContext.Model.GetEntityTypes()
            .FirstOrDefault(e => e.ClrType.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));

        if (entityType == null) return default;

        var method = typeof(DbContext).GetMethod(nameof(DbContext.Set),
                BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null)
            .MakeGenericMethod(entityType.ClrType);

        var dbSet = method.Invoke(_dbContext, null) as DbSet<TEntity>;

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
        await _dbContext.SaveChangesAsync();

        return result.Entity;
    }

    public async Task<TEntity> UpdateAsync(string tableName, TEntity entity)
    {
        try
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
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
            _dbContext.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException e)
        {
            throw new Exception("The DELETE statement conflicted with the REFERENCE constraint");
        }
    }

    public async Task DeleteWithReferencesAsync(string tableName, TEntity entity)
    {
        // 1. Identify and load referencing records
        var references = _dbContext.Model.FindEntityType(typeof(TEntity))
            .GetReferencingForeignKeys();

        foreach (var fk in references)
        {
            var relatedEntityType = fk.DeclaringEntityType.ClrType;
            var setMethod = typeof(DbContext)
                .GetMethod(nameof(DbContext.Set), BindingFlags.Public | BindingFlags.Instance)
                .MakeGenericMethod(relatedEntityType);
            var set = setMethod.Invoke(_dbContext, null);

            var foreignKeyPropertyName = fk.Properties.First().Name;
            var keyValue = entity.GetType().GetProperty("Id").GetValue(entity);

            var whereMethod = typeof(Queryable).GetMethods()
                .First(m => m.Name == "Where" && m.GetParameters().Length == 2)
                .MakeGenericMethod(relatedEntityType);
            var lambda = CreateEqualsExpression(relatedEntityType, foreignKeyPropertyName, keyValue);
            var recordsToDelete = whereMethod.Invoke(null, new object[] { set, lambda }) as IQueryable;

            _dbContext.RemoveRange(recordsToDelete);
        }

        // 2. Delete the main record
        _dbContext.Remove(entity);

        // 3. Save the changes
        await _dbContext.SaveChangesAsync();
    }

    private static LambdaExpression CreateEqualsExpression(Type entityType, string propertyName, object value)
    {
        var param = Expression.Parameter(entityType);
        var prop = Expression.Property(param, propertyName);
        var constValue = Expression.Constant(value);
        var equals = Expression.Equal(prop, constValue);
        return Expression.Lambda(equals, param);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}