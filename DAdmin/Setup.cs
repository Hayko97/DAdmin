using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using DAdmin.Services;
using DAdmin.Services.DbServices;
using DAdmin.Services.DbServices.Interfaces;
using DAdmin.States;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DAdmin;

public static class Setup
{
    public static void DAdmin<TContext>(this IServiceCollection services) where TContext : DbContext
    {
        //DB Services
        services.AddTransient<DbService>();
        services.AddTransient(typeof(IDataService<>), typeof(DataService<>));
        services.AddTransient<IDbInfoService, DbInfoService>();
        services.AddTransient(typeof(IDataMapperService<>), typeof(DataMapperService<>));

        //Component Services
        services.AddTransient<IMenuService, MenuService>();

        //States
        services.AddSingleton<MenuState>();

        services.AddScoped(typeof(DbContext), provider => provider.GetRequiredService<TContext>());

        services
            .AddBlazorise(options => { options.Immediate = true; })
            .AddBootstrapProviders()
            .AddFontAwesomeIcons();
    }
}