using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CalculadoraCostes.Application.Interfaces;
using CalculadoraCostes.Domain.Entities;
using CalculadoraCostes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CalculadoraCostes.Infrastructure.Repositories;

public class EnergyRepository(CalculadoraDbContext context) : IEnergyRepository
{
    public async Task<List<Energy>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Energies
            .Include(e => e.CostComponents)
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);

    public async Task<Energy?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await context.Energies
            .Include(e => e.CostComponents)
            .FirstOrDefaultAsync(e => e.Code == code, cancellationToken);

    public async Task<Energy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Energies
            .Include(e => e.CostComponents)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task AddAsync(Energy energy, CancellationToken cancellationToken = default)
    {
        await context.Energies.AddAsync(energy, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Energy energy, CancellationToken cancellationToken = default)
    {
        context.Energies.Update(energy);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await context.Energies.FindAsync(new object?[] { id }, cancellationToken);
        if (entity is null)
        {
            return;
        }

        context.Energies.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }
}
