using DAdmin.Builders.interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using DAdmin.Dto.Stats;

namespace DAdmin.Builders
{
    public class StatsBuilder : IStatsBuilder
    {
        private Stats _stats = new();
        private DbContext _dbContext;

        public StatsBuilder(DbContext context)
        {
            _dbContext = context;
        }

        public async Task<StatsBuilder> WithGeneralStatsAsync(DateTime? recentActivityThreshold = null)
        {
            if (recentActivityThreshold == null)
            {
                recentActivityThreshold = DateTime.Now.AddDays(-30);
            }

            var dbSetProperties = _dbContext.GetType().GetProperties()
                .Where(p => p.PropertyType.IsGenericType &&
                            p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

            _stats.TotalTables = dbSetProperties.Count();

            foreach (var prop in dbSetProperties)
            {
                var dbSet = prop.GetValue(_dbContext) as IQueryable;
                var entityType = prop.PropertyType.GenericTypeArguments[0];
                var tableName = prop.Name;
                _stats.ActivityPerTableOverTime[tableName] = new Dictionary<DateTime, int>();
                _stats.TableRecordCounts[tableName] = await dbSet.Cast<object>().CountAsync();

                var dateProperty = entityType.GetProperties()
                    .FirstOrDefault(p => new[]
                        {
                            "CreatedAt", "CreatedOn", "CreatedDate", "DateCreated", "Timestamp", "CreationDate",
                            "CreationTime"
                        }
                        .Contains(p.Name) && p.PropertyType == typeof(DateTime));

                if (dateProperty != null)
                {
                    var allDates = await dbSet.Cast<object>()
                        .Select(entity => (DateTime)dateProperty.GetValue(entity))
                        .ToListAsync();

                    _stats.RecentActivityCount += allDates.Count(date => date >= recentActivityThreshold);

                    foreach (var date in allDates)
                    {
                        var activityDate = date.Date;
                        if (activityDate >= recentActivityThreshold)
                        {
                            if (!_stats.ActivityPerTableOverTime[tableName].ContainsKey(activityDate))
                            {
                                _stats.ActivityPerTableOverTime[tableName][activityDate] = 0;
                            }

                            _stats.ActivityPerTableOverTime[tableName][activityDate]++;
                        }
                    }
                }
            }

            _stats.TotalTables = dbSetProperties.Count();
            _stats.RecentActivityCount = 0;
            foreach (var prop in dbSetProperties)
            {
                var dbSet = prop.GetValue(_dbContext) as IQueryable;
                var entityType = prop.PropertyType.GenericTypeArguments[0];
                var tableName = prop.Name;

                _stats.TableRecordCounts[tableName] = await dbSet.Cast<object>().CountAsync();

                var dateProperty = entityType.GetProperties()
                    .FirstOrDefault(p => new[]
                        {
                            "CreatedAt", "CreatedOn", "CreatedDate", "DateCreated", "Timestamp", "CreationDate",
                            "CreationTime"
                        }
                        .Contains(p.Name) && p.PropertyType == typeof(DateTime));

                if (dateProperty != null)
                {
                    var recentItems = await dbSet.Cast<object>()
                        .Select(entity => (DateTime)dateProperty.GetValue(entity))
                        .ToListAsync();

                    _stats.RecentActivityCount += recentItems.Count(date => date >= recentActivityThreshold);
                }
            }

            return this;
        }


        public async Task<StatsBuilder> WithErrorStatsAsync(DateTime? recentErrorThreshold = null)
        {
            if (recentErrorThreshold == null)
            {
                recentErrorThreshold = DateTime.Now.AddDays(-7);
            }

            var errorLogStats = new ErrorLogStats();

            var errorLogTableNames = new[]
                { "ErrorLogs", "ErrorLog", "LogErrors", "SystemErrors", "ErrorEntries", "ErrorRecords", "ErrorList" };

            var errorLogDbSet = _dbContext.GetType().GetProperties()
                .FirstOrDefault(p => errorLogTableNames.Contains(p.Name) &&
                                     p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

            if (errorLogDbSet != null)
            {
                var errorLogs = await ((IQueryable)errorLogDbSet.GetValue(_dbContext)).Cast<dynamic>().ToListAsync();

                errorLogStats.ErrorLogCount = errorLogs.Count;
                errorLogStats.ErrorTypeCounts = errorLogs.GroupBy(log => (string)log.Type)
                    .ToDictionary(g => g.Key, g => g.Count());
                errorLogStats.RecentErrorCount = errorLogs.Count(log => log.DateCreated >= recentErrorThreshold);
                errorLogStats.MostCommonErrors = errorLogs.GroupBy(log => (string)log.Message)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .ToDictionary(g => g.Key, g => g.Count());
            }

            _stats.ErrorLogStats = errorLogStats;
            return this;
        }

        public async Task<StatsBuilder> WithSalesStatsAsync()
        {
            var salesTableNames = new[]
                { "Sales", "Sale", "Transactions", "SalesData", "SalesRecords", "SaleEntries", "SaleTransactions" };
            var salesDbSet = _dbContext.GetType().GetProperties()
                .FirstOrDefault(p => salesTableNames.Contains(p.Name) &&
                                     p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

            var salesStats = new SalesStats();
            if (salesDbSet != null)
            {
                var salesData = await ((IQueryable)salesDbSet.GetValue(_dbContext)).Cast<dynamic>().ToListAsync();

                salesStats.SalesCount = salesData.Count;
                salesStats.TotalSalesAmount = salesData.Sum(sale => (double)sale.Amount);
                salesStats.AverageSaleAmount =
                    salesData.Count > 0 ? salesStats.TotalSalesAmount / salesStats.SalesCount : 0;
            }

            _stats.SalesStats = salesStats;
            return this;
        }

        public Stats Build()
        {
            return _stats;
        }
    }
}