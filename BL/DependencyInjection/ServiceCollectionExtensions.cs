using BL.Interfaces.Repositories;
using BL.Interfaces.Services;
using BL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BL.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlServices(this IServiceCollection services)
    {
        services.AddScoped<IMenuItemService, MenuItemService>();
        return services;
    }
}
