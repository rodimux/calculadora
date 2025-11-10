using System;
using System.Collections.Generic;
using CalculadoraCostes.Domain.Common;
using CalculadoraCostes.Domain.Enums;

namespace CalculadoraCostes.Domain.Entities;

/// <summary>
/// Represents a type of energy/fuel that can be evaluated by the calculator.
/// </summary>
public class Energy : BaseEntity
{
    public string Code { get; set; } = default!;

    public string Name { get; set; } = default!;

    public EnergyFamily Family { get; set; } = EnergyFamily.Otro;

    public EnergyMode Mode { get; set; } = EnergyMode.Simple;

    /// <summary>
    /// When the record represents a DUO configuration, this links to the base simple energy.
    /// </summary>
    public Guid? BaseEnergyId { get; set; }

    /// <summary>
    /// Optional link to another energy that provides emission factors.
    /// </summary>
    public Guid? EmissionReferenceEnergyId { get; set; }

    public decimal PricePerUnit { get; set; }

    /// <summary>
    /// Consumption expressed per 100 km, as defined in the Excel workbook.
    /// </summary>
    public decimal ConsumptionPer100Km { get; set; }

    public decimal RentingCostPerMonth { get; set; }

    /// <summary>
    /// Emission factor in kg of CO2 per unit of energy (litre/kg/kWh).
    /// </summary>
    public decimal EmissionFactorPerUnit { get; set; }

    /// <summary>
    /// Percentage (0-1) of the energy that comes from renewable sources.
    /// </summary>
    public decimal? RenewableShare { get; set; }

    /// <summary>
    /// Percentage (0-1) of emission reduction applied to the renewable share.
    /// </summary>
    public decimal? EmissionReduction { get; set; }

    /// <summary>
    /// Optional flag for energies where the emission factor should be inherited from the base energy.
    /// </summary>
    public bool InheritEmissionFromBase { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<EnergyCostComponent> CostComponents { get; set; } = new List<EnergyCostComponent>();
}
