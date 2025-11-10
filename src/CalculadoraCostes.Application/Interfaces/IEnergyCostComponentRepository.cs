using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CalculadoraCostes.Domain.Entities;

namespace CalculadoraCostes.Application.Interfaces;

public interface IEnergyCostComponentRepository
{
    Task<List<EnergyCostComponent>> GetByEnergyAsync(Guid energyId, CancellationToken cancellationToken = default);

    Task UpsertAsync(EnergyCostComponent component, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
