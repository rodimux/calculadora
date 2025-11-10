using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CalculadoraCostes.Domain.Entities;

namespace CalculadoraCostes.Application.Interfaces;

public interface IEnergyRepository
{
    Task<List<Energy>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Energy?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<Energy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Energy energy, CancellationToken cancellationToken = default);

    Task UpdateAsync(Energy energy, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
