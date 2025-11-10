using System;

namespace CalculadoraCostes.Contracts.Admin;

public class SystemParameterDto
{
    public Guid Id { get; set; }

    public string Key { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string Category { get; set; } = default!;

    public string? Description { get; set; }

    public string? Unit { get; set; }

    public decimal Value { get; set; }

    public bool IsEditable { get; set; }
}
