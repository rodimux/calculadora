using CalculadoraCostes.Domain.Common;
using CalculadoraCostes.Domain.Enums;

namespace CalculadoraCostes.Domain.Entities;

/// <summary>
/// Represents a configurable parameter (constant) used by the calculator.
/// </summary>
public class SystemParameter : BaseEntity
{
    public string Key { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string? Description { get; set; }

    public ParameterCategory Category { get; set; } = ParameterCategory.General;

    public decimal Value { get; set; }

    public string? Unit { get; set; }

    public decimal? MinValue { get; set; }

    public decimal? MaxValue { get; set; }

    public bool IsEditable { get; set; } = true;
}
