using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CalculadoraCostes.Application.Interfaces;
using CalculadoraCostes.Domain.Constants;
using CalculadoraCostes.Domain.Entities;
using CalculadoraCostes.Domain.Enums;
using CalculadoraCostes.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace CalculadoraCostes.Infrastructure.Services;

public class SeedParameterImportService(
    ISystemParameterRepository repository,
    ILogger<SeedParameterImportService> logger) : IParameterImportService
{
    public async Task ImportFromSeedAsync(CancellationToken cancellationToken = default)
    {
        var seedPath = ResolveSeedPath();
        if (!File.Exists(seedPath))
        {
            logger.LogWarning("Seed file not found at {Path}. Unable to import parameters.", seedPath);
            return;
        }

        using var stream = File.OpenRead(seedPath);
        using var document = JsonDocument.Parse(stream);
        var root = document.RootElement;
        if (!root.TryGetProperty("parameters", out var parametersElement))
        {
            logger.LogWarning("Seed file at {Path} does not contain 'parameters' section.", seedPath);
            return;
        }

        foreach (var (property, descriptor) in ParameterDescriptors)
        {
            if (!parametersElement.TryGetProperty(property, out var valueElement))
            {
                continue;
            }

            var parameter = new SystemParameter
            {
                Key = descriptor.Key,
                Name = descriptor.Name,
                Description = descriptor.Description,
                Category = descriptor.Category,
                Unit = descriptor.Unit,
                Value = valueElement.GetDecimal(),
                IsEditable = true
            };

            await repository.UpsertAsync(parameter, cancellationToken);
        }

        logger.LogInformation("Parameters imported from seed file {Path}", seedPath);
    }

    private static string ResolveSeedPath()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var candidate = Path.Combine(baseDirectory, "database", "seed-data.json");
        if (File.Exists(candidate))
        {
            return candidate;
        }

        return Path.GetFullPath(
            Path.Combine(baseDirectory, "..", "..", "..", "..", "database", "seed-data.json"));
    }

    private static readonly Dictionary<string, ParameterDescriptor> ParameterDescriptors = new()
    {
        ["KmsPerDay"] = new(SystemParameterKeys.KmsPerDayDefault, "Kms por día (por defecto)", ParameterCategory.Operation, "Valor inicial sugerido para el formulario de la calculadora", "km"),
        ["DaysPerMonth"] = new(SystemParameterKeys.DaysPerMonthDefault, "Días por mes (por defecto)", ParameterCategory.Operation, "Número de días operativos estimados por mes", "días"),
        ["DriverSalary"] = new(SystemParameterKeys.DriverSalary, "Salario conductor", ParameterCategory.Operation, "Salario mensual por conductor", "€"),
        ["Margin"] = new(SystemParameterKeys.Margin, "Margen comercial", ParameterCategory.Pricing, "Margen aplicado al coste diario", "%"),
        ["TrailerPrice"] = new(SystemParameterKeys.TrailerPrice, "Coste semirremolque", ParameterCategory.Operation, "Cuota mensual semirremolque estándar", "€"),
        ["DollyPrice"] = new(SystemParameterKeys.DollyPrice, "Coste dolly", ParameterCategory.Operation, "Cuota mensual dolly para DUO", "€"),
        ["DuoConsumptionSaving"] = new(SystemParameterKeys.DuoConsumptionSaving, "Ahorro consumo DUO", ParameterCategory.Operation, "Factor de ahorro en consumo energético para DUO", "%"),
        ["YardCost"] = new(SystemParameterKeys.YardCost, "Coste yard", ParameterCategory.Corridor, "Costes adicionales de yard por viaje", "€"),
        ["TransportCost"] = new(SystemParameterKeys.TransportCost, "Coste transporte plaza", ParameterCategory.Corridor, "Coste transporte plaza por viaje", "€"),
        ["DeliveriesPerMonth"] = new(SystemParameterKeys.DeliveriesPerMonth, "Número de descargas", ParameterCategory.Corridor, "Número de descargas mensuales", "unidades"),
        ["CorridorKmDuo"] = new(SystemParameterKeys.CorridorKmDuo, "Kms corredor DUO", ParameterCategory.Corridor, "Kilómetros estimados para corredor DUO", "km"),
        ["TripsPerMonth"] = new(SystemParameterKeys.TripsPerMonth, "Acarreos por viaje", ParameterCategory.Corridor, "Número de acarreos por viaje", "viajes"),
        ["TollKmSimple"] = new(SystemParameterKeys.TollKmSimple, "Kms autopista simple", ParameterCategory.Corridor, "Kilómetros en autopista para vehículo simple", "km"),
        ["TollKmDuo"] = new(SystemParameterKeys.TollKmDuo, "Kms autopista DUO", ParameterCategory.Corridor, "Kilómetros en autopista para vehículo DUO", "km"),
        ["TollPricePerKmSimple"] = new(SystemParameterKeys.TollPricePerKmSimple, "Precio autopista simple", ParameterCategory.Corridor, "Precio por km de autopista (vehículo simple)", "€/km"),
        ["TollPricePerKmDuo"] = new(SystemParameterKeys.TollPricePerKmDuo, "Precio autopista DUO", ParameterCategory.Corridor, "Precio por km de autopista (vehículo DUO)", "€/km"),
        ["ExtraDriverFactor"] = new(SystemParameterKeys.ExtraDriverFactor, "Factor extra conductor", ParameterCategory.Operation, "Sobrecoste por conductor adicional para vehículos especiales", "%"),
        ["PricePerTonCO2"] = new(SystemParameterKeys.PricePerTonCo2, "Precio tonelada CO₂", ParameterCategory.Emissions, "Coste por tonelada de CO₂ emitida", "€/t"),
        ["TariffCorrectionFactor"] = new(SystemParameterKeys.TariffCorrectionFactor, "Factor corrector tarifa", ParameterCategory.Pricing, "Corrección aplicada a la tarifa calculada", "%"),
        ["SecondDriverThreshold"] = new(SystemParameterKeys.SecondDriverThreshold, "Umbral km segundo conductor", ParameterCategory.Operation, "Kilómetros a partir de los cuales se requiere un segundo conductor", "km")
    };

    private record ParameterDescriptor(
        string Key,
        string Name,
        ParameterCategory Category,
        string? Description = null,
        string? Unit = null);
}
