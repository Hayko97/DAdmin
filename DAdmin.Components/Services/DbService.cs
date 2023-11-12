using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DAdmin.Components.Services;

public class DbService : IDisposable
{
    private DbContext _dbContext;
    private IServiceProvider _serviceProvider;

    public DbService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _dbContext = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<DbContext>();
    }

    protected DbContext DbContext => _dbContext;

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}