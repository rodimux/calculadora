using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CalculadoraCostes.Application.Models;

namespace CalculadoraCostes.Application.Interfaces;

public interface ICostCalculationService
{
    Task<CalculationSummary> CalculateAsync(CalculationScenario scenario, CancellationToken cancellationToken = default);
}
