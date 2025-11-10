using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CalculadoraCostes.Domain.Constants;
using CalculadoraCostes.Domain.Entities;
using CalculadoraCostes.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CalculadoraCostes.Infrastructure.Persistence;

public static class DataSeeder
{
    private static readonly Regex NonAlphaNumeric = new("[^a-z0-9]+", RegexOptions.Compiled);

    public static async Task SeedAsync(
        CalculadoraDbContext context,
        ILogger? logger = null,
        CancellationToken cancellationToken = default,
        bool overwriteExisting = false)
    {
        if (overwriteExisting)
        {
            await context.EnergyCostComponents.ExecuteDeleteAsync(cancellationToken);
            await context.Energies.ExecuteDeleteAsync(cancellationToken);
            await context.SystemParameters.ExecuteDeleteAsync(cancellationToken);
        }

        var hasEnergies = !overwriteExisting && await context.Energies.AnyAsync(cancellationToken);
        var hasParameters = !overwriteExisting && await context.SystemParameters.AnyAsync(cancellationToken);
        if (hasEnergies && hasParameters && !overwriteExisting)
        {
            logger?.LogInformation("Seed skipped: energies and parameters already exist.");
            return;
        }

        var jsonPath = ResolveSeedPath();
        if (!File.Exists(jsonPath))
        {
            logger?.LogWarning("Seed file not found at {Path}. Skipping automatic seed.", jsonPath);
            return;
        }

        using var stream = File.OpenRead(jsonPath);
        using var doc = JsonDocument.Parse(stream);

        var root = doc.RootElement;
        var parameterElement = root.GetProperty("parameters");

        if (!hasEnergies)
        {
            var energyElements = root.GetProperty("energies").EnumerateArray().ToList();
            var componentMap = root.GetProperty("components");
            var energies = CreateEnergyEntities(energyElements);

            ApplyRelationships(energies);

            foreach (var energy in energies.Values)
            {
                if (componentMap.TryGetProperty(energy.Name, out var componentsElement))
                {
                    var order = 0;
                    foreach (var component in componentsElement.EnumerateArray())
                    {
                        var value = component.GetProperty("value").GetDecimal();
                        if (value == 0)
                        {
                            continue;
                        }

                        var comp = new EnergyCostComponent
                        {
                            EnergyId = energy.Id,
                            Name = component.GetProperty("name").GetString() ?? string.Empty,
                            Key = GenerateKey(component.GetProperty("name").GetString() ?? string.Empty),
                            Category = ParseCostCategory(component.GetProperty("category").GetString()),
                            ValueType = ParseValueType(component.GetProperty("value_type").GetString()),
                            Value = value,
                            Order = order++
                        };
                        energy.CostComponents.Add(comp);
                    }
                }
            }

            await context.Energies.AddRangeAsync(energies.Values, cancellationToken);
        }

        if (!hasParameters)
        {
            var parameters = CreateParameters(parameterElement);
            await context.SystemParameters.AddRangeAsync(parameters, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        logger?.LogInformation("Seed data imported from {Path}", jsonPath);
    }

    private static Dictionary<string, Energy> CreateEnergyEntities(IEnumerable<JsonElement> elements)
    {
        var map = new Dictionary<string, Energy>(StringComparer.OrdinalIgnoreCase);
        foreach (var element in elements)
        {
            var name = element.GetProperty("name").GetString() ?? string.Empty;
            var code = GenerateCode(name);
            var config = GetEnergyConfig(name);
            var consumptionToken = element.GetProperty("consumption_per_100km");
            var rentToken = element.GetProperty("rent");

            var energy = new Energy
            {
                Id = Guid.NewGuid(),
                Name = name,
                Code = code,
                Family = config.Family,
                Mode = config.Mode,
                PricePerUnit = element.GetProperty("price").GetDecimal(),
                ConsumptionPer100Km = consumptionToken.ValueKind == JsonValueKind.Null ? 0m : consumptionToken.GetDecimal(),
                RentingCostPerMonth = rentToken.ValueKind == JsonValueKind.Null ? 0m : rentToken.GetDecimal(),
                EmissionFactorPerUnit = config.EmissionFactor,
                RenewableShare = config.RenewableShare,
                EmissionReduction = config.EmissionReduction,
                InheritEmissionFromBase = config.InheritEmission
            };

            map.Add(name, energy);
        }

        return map;
    }

    private static void ApplyRelationships(Dictionary<string, Energy> energies)
    {
        foreach (var (name, config) in EnergyConfigs)
        {
            if (!energies.TryGetValue(name, out var energy))
            {
                continue;
            }

            if (config.BaseEnergy is not null && energies.TryGetValue(config.BaseEnergy, out var baseEnergy))
            {
                energy.BaseEnergyId = baseEnergy.Id;
            }

            if (config.EmissionReference is not null && energies.TryGetValue(config.EmissionReference, out var emissionRef))
            {
                energy.EmissionReferenceEnergyId = emissionRef.Id;
            }
        }
    }

    private static IEnumerable<SystemParameter> CreateParameters(JsonElement parametersElement)
    {
        foreach (var (property, descriptor) in ParameterDescriptors)
        {
            if (!parametersElement.TryGetProperty(property, out var valueElement))
            {
                continue;
            }

            yield return new SystemParameter
            {
                Id = Guid.NewGuid(),
                Key = descriptor.Key,
                Name = descriptor.Name,
                Category = descriptor.Category,
                Description = descriptor.Description,
                Unit = descriptor.Unit,
                Value = valueElement.GetDecimal(),
                IsEditable = true
            };
        }
    }

    private static string ResolveSeedPath()
    {
        var baseDirectory = AppContext.BaseDirectory;
        var candidate = Path.Combine(baseDirectory, "database", "seed-data.json");
        if (File.Exists(candidate))
        {
            return candidate;
        }

        // Fallback to project root (useful for design-time seeding).
        return Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "database", "seed-data.json"));
    }

    private static string GenerateCode(string name)
        => GenerateKey(name).Replace(".", "_").ToUpperInvariant();

    private static string GenerateKey(string input)
    {
        var normalized = input
            .Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .Aggregate(new StringBuilder(), (sb, c) => sb.Append(char.ToLowerInvariant(c)))
            .ToString();

        normalized = NonAlphaNumeric.Replace(normalized, ".");
        return normalized.Trim('.');
    }

    private static CostComponentCategory ParseCostCategory(string? value)
        => value switch
        {
            "Fixed" => CostComponentCategory.Fixed,
            "Variable" => CostComponentCategory.Variable,
            "Overhead" => CostComponentCategory.Overhead,
            _ => CostComponentCategory.Fixed
        };

    private static CostComponentValueType ParseValueType(string? value)
        => value switch
        {
            "Monthly" => CostComponentValueType.MonthlyAmount,
            "PerKilometerRate" => CostComponentValueType.PerKilometerRate,
            "Percentage" => CostComponentValueType.PercentageOverSubtotal,
            _ => CostComponentValueType.MonthlyAmount
        };

    private static EnergyConfigurationDescriptor GetEnergyConfig(string name)
        => EnergyConfigs.TryGetValue(name, out var config)
            ? config
            : EnergyConfigurationDescriptor.Default;

    private static readonly Dictionary<string, EnergyConfigurationDescriptor> EnergyConfigs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["DIESEL"] = new(EnergyFamily.Diesel) { EmissionFactor = 2.493m },
        ["GAS NATURAL"] = new(EnergyFamily.GasNatural) { EmissionFactor = 2.721m },
        ["H2"] = new(EnergyFamily.Hidrogeno) { EmissionFactor = 0m },
        ["BIOMETANO"] = new(EnergyFamily.Biometano)
        {
            EmissionFactor = 2.721m,
            RenewableShare = 1m,
            EmissionReduction = 0.9m,
            EmissionReference = "GAS NATURAL"
        },
        ["ELECTRICO"] = new(EnergyFamily.Electrico) { EmissionFactor = 0m },
        ["HVO"] = new(EnergyFamily.Hvo)
        {
            EmissionFactor = 2.493m,
            RenewableShare = 1m,
            EmissionReduction = 0.9m,
            EmissionReference = "DIESEL"
        },
        ["DUO H2"] = new(EnergyFamily.Hidrogeno, EnergyMode.Duo)
        {
            BaseEnergy = "H2",
            EmissionFactor = 0m
        },
        ["DUO GASOIL"] = new(EnergyFamily.Diesel, EnergyMode.Duo)
        {
            BaseEnergy = "DIESEL",
            EmissionFactor = 2.493m,
            InheritEmission = true
        },
        ["DUO BIOMETANO"] = new(EnergyFamily.Biometano, EnergyMode.Duo)
        {
            BaseEnergy = "BIOMETANO",
            EmissionFactor = 2.721m,
            RenewableShare = 1m,
            EmissionReduction = 0.9m,
            InheritEmission = true
        },
        ["DUO HVO"] = new(EnergyFamily.Hvo, EnergyMode.Duo)
        {
            BaseEnergy = "HVO",
            EmissionFactor = 2.493m,
            RenewableShare = 1m,
            EmissionReduction = 0.9m,
            InheritEmission = true
        },
        ["DUO ELECTRICO"] = new(EnergyFamily.Electrico, EnergyMode.Duo)
        {
            BaseEnergy = "ELECTRICO",
            EmissionFactor = 0m
        }
    };

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

    private record EnergyConfigurationDescriptor(
        EnergyFamily Family,
        EnergyMode Mode = EnergyMode.Simple)
    {
        public static readonly EnergyConfigurationDescriptor Default = new(EnergyFamily.Otro);

        public decimal EmissionFactor { get; init; } = 0m;

        public decimal? RenewableShare { get; init; }

        public decimal? EmissionReduction { get; init; }

        public string? BaseEnergy { get; init; }

        public string? EmissionReference { get; init; }

        public bool InheritEmission { get; init; }
    }

    private record ParameterDescriptor(
        string Key,
        string Name,
        ParameterCategory Category,
        string? Description = null,
        string? Unit = null);
}
