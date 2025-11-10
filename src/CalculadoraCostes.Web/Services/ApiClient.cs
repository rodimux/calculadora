using System.Net.Http.Json;
using CalculadoraCostes.Contracts.Admin;
using CalculadoraCostes.Contracts.Calculator;

namespace CalculadoraCostes.Web.Services;

public class ApiClient(HttpClient httpClient)
{
    public async Task<CalculationResponseDto?> CalculateAsync(CalculationRequestDto request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/calculator", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CalculationResponseDto>(cancellationToken: cancellationToken);
    }

    public async Task<List<EnergyDto>> GetEnergiesAsync(CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<List<EnergyDto>>("api/admin/energies", cancellationToken);
        return result ?? [];
    }

    public async Task UpdateEnergyAsync(Guid id, EnergyDto dto, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/admin/energies/{id}", dto, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<EnergyDto> CreateEnergyAsync(EnergyDto dto, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/admin/energies", dto, cancellationToken);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<EnergyDto>(cancellationToken: cancellationToken);
        return created ?? dto;
    }

    public async Task DeleteEnergyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/admin/energies/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<EnergyCostComponentDto?> UpsertComponentAsync(Guid energyId, EnergyCostComponentDto dto, CancellationToken cancellationToken = default)
    {
        var path = dto.Id == Guid.Empty
            ? $"api/admin/energies/{energyId}/components"
            : $"api/admin/energies/{energyId}/components/{dto.Id}";

        HttpResponseMessage response;
        if (dto.Id == Guid.Empty)
        {
            response = await httpClient.PostAsJsonAsync(path, dto, cancellationToken);
        }
        else
        {
            response = await httpClient.PutAsJsonAsync(path, dto, cancellationToken);
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EnergyCostComponentDto>(cancellationToken: cancellationToken);
    }

    public async Task DeleteComponentAsync(Guid energyId, Guid componentId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"api/admin/energies/{energyId}/components/{componentId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<SystemParameterDto>> GetParametersAsync(CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<List<SystemParameterDto>>("api/admin/parameters", cancellationToken);
        return result ?? [];
    }

    public async Task UpdateParameterAsync(string key, SystemParameterDto dto, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/admin/parameters/{key}", dto, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task ImportParametersAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync("api/admin/parameters/import", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task ImportEnergiesAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync("api/admin/energies/import", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
