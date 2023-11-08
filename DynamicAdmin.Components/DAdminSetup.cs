using DynamicAdmin.Components.Services;
using DynamicAdmin.Components.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DynamicAdmin.Components;

public static class DAdminSetup
{
    public static void SetupAdminPanel<TContext>(this IServiceCollection services) where TContext : DbContext
    {
        services.AddTransient<DbService>();
        services.AddTransient(typeof(IDataService<>), typeof(DataService<>));
        services.AddTransient<IDbInfoService, DbInfoService>();
        services.AddTransient(typeof(IDataMapperService<>), typeof(DataMapperService<>));

        services.AddScoped(typeof(DbContext), provider => provider.GetRequiredService<TContext>());

        services.AddMvcCore().AddRazorPages()
            .AddApplicationPart(typeof(DynamicAdmin.Components.Pages.CreateEntity).Assembly);
    }
}