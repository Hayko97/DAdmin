using DynamicAdmin.Components.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DynamicAdmin.Components;

public static class DAdminSetup
{
    public static void SetupAdminPanel<TContext>(this IServiceCollection services) where TContext : DbContext
    {
        services.AddScoped(typeof(IDataService<>), typeof(DataService<>));
        services.AddScoped(typeof(DbContext), provider => provider.GetRequiredService<TContext>());
    }
}