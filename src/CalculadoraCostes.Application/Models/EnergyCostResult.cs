using CalculadoraCostes.Domain.Enums;

namespace CalculadoraCostes.Application.Models;

public sealed record EnergyCostResult(
    string EnergyCode,
    string EnergyName,
    EnergyMode Mode,
    EnergyFamily Family,
    decimal EnergyCostPerKm,
    decimal CarbonCostPerKm,
    decimal OperatingCostPerKm,
    decimal TotalCostPerKm,
    decimal CostPerDay,
    decimal TariffSuggested,
    decimal EmissionsKgPerKm,
    decimal? EmissionReductionVsDiesel,
    decimal ExtraCostVsDieselPercentage,
    decimal? ExtraCostVsDuoDiesel);
