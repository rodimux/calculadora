using CalculadoraCostes.Application.Interfaces;
using CalculadoraCostes.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CalculadoraCostes.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICostCalculationService, CostCalculationService>();
        return services;
    }
}
