using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CalculadoraCostes.Application.Interfaces;
using CalculadoraCostes.Application.Models;
using CalculadoraCostes.Domain.Constants;
using CalculadoraCostes.Domain.Entities;
using CalculadoraCostes.Domain.Enums;

namespace CalculadoraCostes.Application.Services;

public class CostCalculationService(
    IEnergyRepository energyRepository,
    ISystemParameterRepository parameterRepository) : ICostCalculationService
{
public async Task<CalculationSummary> CalculateAsync(CalculationScenario scenario, CancellationToken cancellationToken = default)
{
        var energies = await energyRepository.GetAllAsync(cancellationToken);
        var parameters = await parameterRepository.GetAllAsync(cancellationToken);
        var parameterMap = parameters.ToDictionary(p => p.Key);

        var kmsPerDay = scenario.KmsPerDay > 0 ? scenario.KmsPerDay : GetParameterValue(parameterMap, SystemParameterKeys.KmsPerDayDefault);
        var daysPerMonth = scenario.DaysPerMonth > 0 ? scenario.DaysPerMonth : GetParameterValue(parameterMap, SystemParameterKeys.DaysPerMonthDefault);
        var margin = scenario.MarginOverride ?? GetParameterValue(parameterMap, SystemParameterKeys.Margin);
        var pricePerTonCo2 = scenario.PricePerTonCo2Override ?? GetParameterValue(parameterMap, SystemParameterKeys.PricePerTonCo2);
        var tariffCorrection = GetParameterValue(parameterMap, SystemParameterKeys.TariffCorrectionFactor);

        var monthlyKm = kmsPerDay * daysPerMonth;
        if (monthlyKm <= 0)
        {
            return new CalculationSummary([], kmsPerDay, daysPerMonth);
        }

        var energyById = energies.ToDictionary(e => e.Id);
        var emissionPerKm = new Dictionary<Guid, decimal>();
        var results = new List<EnergyCostResult>();

        // Process simple energies first to ensure base values exist for DUO variants.
        foreach (var energy in energies.Where(e => e.Mode == EnergyMode.Simple))
        {
            var result = ComputeResult(energy, energyById, emissionPerKm, kmsPerDay, monthlyKm, margin, pricePerTonCo2);
            emissionPerKm[energy.Id] = result.EmissionsKgPerKm;
            results.Add(result);
        }

        foreach (var energy in energies.Where(e => e.Mode == EnergyMode.Duo))
        {
            var result = ComputeResult(energy, energyById, emissionPerKm, kmsPerDay, monthlyKm, margin, pricePerTonCo2);
            emissionPerKm[energy.Id] = result.EmissionsKgPerKm;
            results.Add(result);
        }

        var diesel = results.FirstOrDefault(r => r.EnergyCode == "DIESEL");
        var duoDiesel = results.FirstOrDefault(r => r.EnergyCode == "DUO_GASOIL");

        var orderedResults = results
            .Select(r =>
            {
                var extraVsDiesel = diesel is null || diesel.TotalCostPerKm == 0
                    ? 0
                    : r.TotalCostPerKm / diesel.TotalCostPerKm - 1;

                var emissionReduction = diesel is null || diesel.EmissionsKgPerKm == 0
                    ? (decimal?)null
                    : (diesel.EmissionsKgPerKm - r.EmissionsKgPerKm) / diesel.EmissionsKgPerKm;

                var extraVsDuoDiesel = duoDiesel is null || duoDiesel.TotalCostPerKm == 0
                    ? (decimal?)null
                    : r.TotalCostPerKm / duoDiesel.TotalCostPerKm - 1;

                return r with
                {
                    ExtraCostVsDieselPercentage = extraVsDiesel,
                    EmissionReductionVsDiesel = emissionReduction,
                    ExtraCostVsDuoDiesel = extraVsDuoDiesel
                };
            })
            .OrderBy(r => r.TotalCostPerKm)
            .ToList();

        return new CalculationSummary(orderedResults, kmsPerDay, daysPerMonth);
}

    private EnergyCostResult ComputeResult(
        Energy energy,
        IDictionary<Guid, Energy> energyById,
        IDictionary<Guid, decimal> emissionPerKmCache,
        decimal kmsPerDay,
        decimal monthlyKm,
        decimal margin,
        decimal pricePerTonCo2)
    {
        var consumptionPerKm = energy.ConsumptionPer100Km / 100m;
        var energyCostPerKm = energy.PricePerUnit * consumptionPerKm;

        var costTotals = AggregateCosts(energy.CostComponents, monthlyKm);
        var co2EmissionPerKm = CalculateEmissionsPerKm(energy, energyById, emissionPerKmCache, consumptionPerKm);
        var carbonCostPerKm = pricePerTonCo2 <= 0 ? 0 : pricePerTonCo2 / 1000m * co2EmissionPerKm;
        var carbonCostPerMonth = carbonCostPerKm * monthlyKm;

        var totalMonthly = costTotals.TotalMonthly + carbonCostPerMonth;
        var totalCostPerKm = totalMonthly / monthlyKm;
        var costPerDay = totalCostPerKm * kmsPerDay;
        var tariff = costPerDay * (1 + margin);

        return new EnergyCostResult(
            EnergyCode: energy.Code,
            EnergyName: energy.Name,
            Mode: energy.Mode,
            Family: energy.Family,
            EnergyCostPerKm: energyCostPerKm,
            CarbonCostPerKm: carbonCostPerKm,
            OperatingCostPerKm: costTotals.TotalMonthly / monthlyKm,
            TotalCostPerKm: totalCostPerKm,
            CostPerDay: costPerDay,
            TariffSuggested: tariff,
            EmissionsKgPerKm: co2EmissionPerKm,
            EmissionReductionVsDiesel: null,
            ExtraCostVsDieselPercentage: 0,
            ExtraCostVsDuoDiesel: null);
    }

    private static decimal CalculateEmissionsPerKm(
        Energy energy,
        IDictionary<Guid, Energy> energyById,
        IDictionary<Guid, decimal> emissionPerKmCache,
        decimal consumptionPerKm)
    {
        if (energy.Mode == EnergyMode.Duo)
        {
            return CalculateDuoEmissions(energy, energyById, emissionPerKmCache, consumptionPerKm);
        }

        if (energy.EmissionFactorPerUnit <= 0)
        {
            return 0;
        }

        var factor = energy.EmissionFactorPerUnit;
        if (energy.EmissionReferenceEnergyId is not null && energyById.TryGetValue(energy.EmissionReferenceEnergyId.Value, out var reference))
        {
            factor = reference.EmissionFactorPerUnit;
        }

        var renewable = energy.RenewableShare ?? 0;
        var reduction = energy.EmissionReduction ?? 0;
        var fossilPortion = 1 - renewable;
        var renewablePortion = renewable * (1 - reduction);

        return consumptionPerKm * factor * (fossilPortion + renewablePortion);
    }

    private static decimal CalculateDuoEmissions(
        Energy energy,
        IDictionary<Guid, Energy> energyById,
        IDictionary<Guid, decimal> emissionPerKmCache,
        decimal consumptionPerKm)
    {
        return energy.Code switch
        {
            "DUO_GASOIL" => consumptionPerKm * GetBaseFactor(energy, energyById) / 2m,
            "DUO_BIOMETANO" => energy.BaseEnergyId is not null && emissionPerKmCache.TryGetValue(energy.BaseEnergyId.Value, out var baseEmission)
                ? baseEmission * 0.7m
                : 0m,
            "DUO_HVO" => consumptionPerKm * 0.174m,
            "DUO_ELECTRICO" => energy.BaseEnergyId is not null && emissionPerKmCache.TryGetValue(energy.BaseEnergyId.Value, out var baseElectric)
                ? baseElectric
                : 0m,
            _ => 0m
        };
    }

    private static decimal GetBaseFactor(Energy energy, IDictionary<Guid, Energy> energyById)
    {
        if (energy.EmissionReferenceEnergyId is not null && energyById.TryGetValue(energy.EmissionReferenceEnergyId.Value, out var reference))
        {
            return reference.EmissionFactorPerUnit;
        }

        if (energy.BaseEnergyId is not null && energyById.TryGetValue(energy.BaseEnergyId.Value, out var baseEnergy))
        {
            return baseEnergy.EmissionFactorPerUnit;
        }

        return energy.EmissionFactorPerUnit;
    }

    private static CostTotals AggregateCosts(IEnumerable<EnergyCostComponent> components, decimal monthlyKm)
    {
        decimal fixedMonthly = 0, variableMonthly = 0, overheadMonthly = 0, overheadPercent = 0;

        foreach (var component in components)
        {
            var target = component.Category switch
            {
                CostComponentCategory.Fixed => CostTarget.Fixed,
                CostComponentCategory.Variable => CostTarget.Variable,
                CostComponentCategory.Overhead => component.ValueType == CostComponentValueType.PercentageOverSubtotal
                    ? CostTarget.OverheadPercentage
                    : CostTarget.Overhead,
                _ => CostTarget.Fixed
            };

            switch (component.ValueType)
            {
                case CostComponentValueType.MonthlyAmount:
                    var value = component.Value;
                    Assign(ref fixedMonthly, ref variableMonthly, ref overheadMonthly, target, value);
                    break;
                case CostComponentValueType.PerKilometerRate:
                    var monthlyValue = component.Value * monthlyKm;
                    Assign(ref fixedMonthly, ref variableMonthly, ref overheadMonthly, target, monthlyValue);
                    break;
                case CostComponentValueType.PercentageOverSubtotal:
                    overheadPercent += component.Value;
                    break;
            }
        }

        var baseMonthly = fixedMonthly + variableMonthly + overheadMonthly;
        var percentageMonthly = baseMonthly * overheadPercent;
        var totalMonthly = baseMonthly + percentageMonthly;

        return new CostTotals(totalMonthly, fixedMonthly, variableMonthly, overheadMonthly, percentageMonthly);
    }

    private static void Assign(ref decimal fixedMonthly, ref decimal variableMonthly, ref decimal overheadMonthly, CostTarget target, decimal value)
    {
        switch (target)
        {
            case CostTarget.Fixed:
                fixedMonthly += value;
                break;
            case CostTarget.Variable:
                variableMonthly += value;
                break;
            case CostTarget.Overhead:
                overheadMonthly += value;
                break;
        }
    }

    private static decimal GetParameterValue(IReadOnlyDictionary<string, Domain.Entities.SystemParameter> parameters, string key)
        => parameters.TryGetValue(key, out var parameter)
            ? parameter.Value
            : 0m;

    private enum CostTarget
    {
        Fixed,
        Variable,
        Overhead,
        OverheadPercentage
    }

    private readonly record struct CostTotals(
        decimal TotalMonthly,
        decimal FixedMonthly,
        decimal VariableMonthly,
        decimal OverheadMonthly,
        decimal PercentageMonthly);
}
