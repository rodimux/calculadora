using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CalculadoraCostes.Domain.Entities;

namespace CalculadoraCostes.Application.Interfaces;

public interface ISystemParameterRepository
{
    Task<List<SystemParameter>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<SystemParameter?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    Task UpsertAsync(SystemParameter parameter, CancellationToken cancellationToken = default);
}
