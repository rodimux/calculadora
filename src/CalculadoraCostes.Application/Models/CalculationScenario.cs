using CalculadoraCostes.Domain.Enums;

namespace CalculadoraCostes.Application.Models;

public sealed record CalculationScenario(
    decimal KmsPerDay,
    decimal DaysPerMonth,
    TrailerType TrailerType,
    decimal? MarginOverride = null,
    decimal? PricePerTonCo2Override = null);
