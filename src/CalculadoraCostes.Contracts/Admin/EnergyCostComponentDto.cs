using System;

namespace CalculadoraCostes.Contracts.Admin;

public class EnergyCostComponentDto
{
    public Guid Id { get; set; }

    public Guid EnergyId { get; set; }

    public string Key { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string Category { get; set; } = default!;

    public string ValueType { get; set; } = default!;

    public decimal Value { get; set; }

    public int Order { get; set; }

    public bool IsEditable { get; set; }

    public string? Notes { get; set; }
}
