using BL.Interfaces.Repositories;
using DAL.Infrastructure;
using DAL.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;

namespace DAL.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDalServices(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(connectionString));
        services.AddScoped<IDbSession>(_ => new DbSession(connectionString));
        services.AddScoped<IMenuItemRepository, MenuItemRepository>();
        return services;
    }
}
