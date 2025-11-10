using System;

namespace CalculadoraCostes.Domain.Common;

/// <summary>
/// Base class for aggregate roots and entities persisted in the database.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }
}
