using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CalculadoraCostes.Application.Interfaces;
using CalculadoraCostes.Domain.Entities;
using CalculadoraCostes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CalculadoraCostes.Infrastructure.Repositories;

public class EnergyCostComponentRepository(CalculadoraDbContext context) : IEnergyCostComponentRepository
{
    public async Task<List<EnergyCostComponent>> GetByEnergyAsync(Guid energyId, CancellationToken cancellationToken = default)
        => await context.EnergyCostComponents
            .Where(c => c.EnergyId == energyId)
            .OrderBy(c => c.Order)
            .ToListAsync(cancellationToken);

    public async Task UpsertAsync(EnergyCostComponent component, CancellationToken cancellationToken = default)
    {
        var existing = await context.EnergyCostComponents
            .FirstOrDefaultAsync(c => c.Id == component.Id, cancellationToken);

        if (existing is null)
        {
            await context.EnergyCostComponents.AddAsync(component, cancellationToken);
        }
        else
        {
            existing.Name = component.Name;
            existing.Category = component.Category;
            existing.ValueType = component.ValueType;
            existing.Value = component.Value;
            existing.Order = component.Order;
            existing.IsEditable = component.IsEditable;
            existing.Notes = component.Notes;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var component = await context.EnergyCostComponents.FindAsync(new object?[] { id }, cancellationToken);
        if (component is null)
        {
            return;
        }

        context.EnergyCostComponents.Remove(component);
        await context.SaveChangesAsync(cancellationToken);
    }
}
