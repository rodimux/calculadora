using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CalculadoraCostes.Application.Interfaces;
using CalculadoraCostes.Domain.Entities;
using CalculadoraCostes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CalculadoraCostes.Infrastructure.Repositories;

public class SystemParameterRepository(CalculadoraDbContext context) : ISystemParameterRepository
{
    public async Task<List<SystemParameter>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.SystemParameters
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);

    public async Task<SystemParameter?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
        => await context.SystemParameters
            .FirstOrDefaultAsync(p => p.Key == key, cancellationToken);

    public async Task UpsertAsync(SystemParameter parameter, CancellationToken cancellationToken = default)
    {
        var existing = await GetByKeyAsync(parameter.Key, cancellationToken);
        if (existing is null)
        {
            await context.SystemParameters.AddAsync(parameter, cancellationToken);
        }
        else
        {
            existing.Value = parameter.Value;
            existing.Description = parameter.Description;
            existing.Name = parameter.Name;
            existing.Unit = parameter.Unit;
            existing.MinValue = parameter.MinValue;
            existing.MaxValue = parameter.MaxValue;
            existing.IsEditable = parameter.IsEditable;
            existing.Category = parameter.Category;
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
