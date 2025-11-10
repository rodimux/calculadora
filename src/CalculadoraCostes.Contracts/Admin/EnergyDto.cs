using System;
using System.Collections.Generic;

namespace CalculadoraCostes.Contracts.Admin;

public class EnergyDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string Mode { get; set; } = default!;

    public string Family { get; set; } = default!;

    public decimal PricePerUnit { get; set; }

    public decimal ConsumptionPer100Km { get; set; }

    public decimal RentingCostPerMonth { get; set; }

    public decimal EmissionFactorPerUnit { get; set; }

    public decimal? RenewableShare { get; set; }

    public decimal? EmissionReduction { get; set; }

    public Guid? BaseEnergyId { get; set; }

    public Guid? EmissionReferenceEnergyId { get; set; }

    public bool InheritEmissionFromBase { get; set; }

    public bool IsActive { get; set; }

    public List<EnergyCostComponentDto> CostComponents { get; set; } = [];
}
