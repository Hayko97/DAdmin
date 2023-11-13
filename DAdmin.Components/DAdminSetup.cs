using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using DAdmin.Components.Services;
using DAdmin.Components.Services.Interfaces;
using DAdmin.Components.States;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DAdmin.Components;

public static class DAdminSetup
{
    public static void SetupAdminPanel<TContext>(this IServiceCollection services) where TContext : DbContext
    {
        services.AddTransient<DbService>();
        services.AddTransient(typeof(IDataService<>), typeof(DataService<>));
        services.AddTransient<IDbInfoService, DbInfoService>();
        services.AddTransient(typeof(IDataMapperService<>), typeof(DataMapperService<>));

        //States
        services.AddSingleton<MenuState>();

        services.AddScoped(typeof(DbContext), provider => provider.GetRequiredService<TContext>());

        services
            .AddBlazorise(options => { options.Immediate = true; })
            .AddBootstrapProviders()
            .AddFontAwesomeIcons();
    }
}