using System;
using CalculadoraCostes.Application.Interfaces;
using CalculadoraCostes.Infrastructure.Persistence;
using CalculadoraCostes.Infrastructure.Repositories;
using CalculadoraCostes.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CalculadoraCostes.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<CalculadoraDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IEnergyRepository, EnergyRepository>();
        services.AddScoped<ISystemParameterRepository, SystemParameterRepository>();
        services.AddScoped<IEnergyCostComponentRepository, EnergyCostComponentRepository>();
        services.AddScoped<IParameterImportService, SeedParameterImportService>();

        return services;
    }
}
