using System.Threading;
using System.Threading.Tasks;

namespace CalculadoraCostes.Application.Interfaces;

public interface IParameterImportService
{
    Task ImportFromSeedAsync(CancellationToken cancellationToken = default);
}
