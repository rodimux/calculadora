using System;
using CalculadoraCostes.Domain.Common;
using CalculadoraCostes.Domain.Enums;

namespace CalculadoraCostes.Domain.Entities;

/// <summary>
/// Describes a cost component that contributes to the cost per km for a given energy type.
/// </summary>
public class EnergyCostComponent : BaseEntity
{
    public Guid EnergyId { get; set; }

    public Energy Energy { get; set; } = default!;

    public string Key { get; set; } = default!;

    public string Name { get; set; } = default!;

    public CostComponentCategory Category { get; set; }

    public CostComponentValueType ValueType { get; set; }

    /// <summary>
    /// Value semantics depend on <see cref="ValueType"/>.
    /// Monthly amounts are stored in EUR/month, per km rates in EUR/km, percentages as 0-1.
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Optional ordering hint for UI display.
    /// </summary>
    public int Order { get; set; }

    public bool IsEditable { get; set; } = true;

    public string? Notes { get; set; }
}
