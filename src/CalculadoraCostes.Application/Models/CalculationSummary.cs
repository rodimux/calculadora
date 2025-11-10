using System.Collections.Generic;

namespace CalculadoraCostes.Application.Models;

public sealed record CalculationSummary(
    IReadOnlyList<EnergyCostResult> Results,
    decimal KmsPerDay,
    decimal DaysPerMonth);
