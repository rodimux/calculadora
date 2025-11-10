namespace CalculadoraCostes.Contracts.Calculator;

public class CalculationResultDto
{
    public string EnergyCode { get; set; } = default!;

    public string EnergyName { get; set; } = default!;

    public string Mode { get; set; } = default!;

    public string Family { get; set; } = default!;

    public decimal EnergyCostPerKm { get; set; }

    public decimal CarbonCostPerKm { get; set; }

    public decimal OperatingCostPerKm { get; set; }

    public decimal TotalCostPerKm { get; set; }

    public decimal CostPerDay { get; set; }

    public decimal TariffSuggested { get; set; }

    public decimal EmissionsKgPerKm { get; set; }

    public decimal? EmissionReductionVsDiesel { get; set; }

    public decimal ExtraCostVsDieselPercentage { get; set; }

    public decimal? ExtraCostVsDuoDiesel { get; set; }
}
