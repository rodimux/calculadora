using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CalculadoraCostes.Application.Models;
using CalculadoraCostes.Contracts;
using CalculadoraCostes.Contracts.Admin;
using CalculadoraCostes.Contracts.Calculator;
using CalculadoraCostes.Domain.Entities;
using CalculadoraCostes.Domain.Enums;

namespace CalculadoraCostes.Api.Mapping;

public static class ApiMappings
{
    public static CalculationResponseDto ToDto(this CalculationSummary summary)
        => new()
        {
            Results = summary.Results.Select(ToDto).ToList(),
            KmsPerDay = summary.KmsPerDay,
            DaysPerMonth = summary.DaysPerMonth
        };

    public static CalculationResultDto ToDto(this EnergyCostResult result)
        => new()
        {
            EnergyCode = result.EnergyCode,
            EnergyName = result.EnergyName,
            Mode = result.Mode.ToString(),
            Family = result.Family.ToString(),
            EnergyCostPerKm = decimal.Round(result.EnergyCostPerKm, 4),
            CarbonCostPerKm = decimal.Round(result.CarbonCostPerKm, 4),
            OperatingCostPerKm = decimal.Round(result.OperatingCostPerKm, 4),
            TotalCostPerKm = decimal.Round(result.TotalCostPerKm, 4),
            CostPerDay = decimal.Round(result.CostPerDay, 2),
            TariffSuggested = decimal.Round(result.TariffSuggested, 2),
            EmissionsKgPerKm = decimal.Round(result.EmissionsKgPerKm, 4),
            EmissionReductionVsDiesel = result.EmissionReductionVsDiesel.HasValue ? decimal.Round(result.EmissionReductionVsDiesel.Value, 4) : null,
            ExtraCostVsDieselPercentage = decimal.Round(result.ExtraCostVsDieselPercentage, 4),
            ExtraCostVsDuoDiesel = result.ExtraCostVsDuoDiesel.HasValue ? decimal.Round(result.ExtraCostVsDuoDiesel.Value, 4) : null
        };

    public static TrailerType ToDomain(this TrailerTypeDto dto)
        => dto switch
        {
            TrailerTypeDto.Dolly => TrailerType.Dolly,
            _ => TrailerType.Trailer
        };

    public static EnergyDto ToDto(this Energy energy)
        => new()
        {
            Id = energy.Id,
            Code = energy.Code,
            Name = energy.Name,
            Mode = energy.Mode.ToString(),
            Family = energy.Family.ToString(),
            PricePerUnit = energy.PricePerUnit,
            ConsumptionPer100Km = energy.ConsumptionPer100Km,
            RentingCostPerMonth = energy.RentingCostPerMonth,
            EmissionFactorPerUnit = energy.EmissionFactorPerUnit,
            RenewableShare = energy.RenewableShare,
            EmissionReduction = energy.EmissionReduction,
            BaseEnergyId = energy.BaseEnergyId,
            EmissionReferenceEnergyId = energy.EmissionReferenceEnergyId,
            InheritEmissionFromBase = energy.InheritEmissionFromBase,
            IsActive = energy.IsActive,
            CostComponents = energy.CostComponents
                .OrderBy(c => c.Order)
                .Select(c => new EnergyCostComponentDto
                {
                    Id = c.Id,
                    EnergyId = c.EnergyId,
                    Key = c.Key,
                    Name = c.Name,
                    Category = c.Category.ToString(),
                    ValueType = c.ValueType.ToString(),
                    Value = c.Value,
                    Order = c.Order,
                    IsEditable = c.IsEditable,
                    Notes = c.Notes
                })
                .ToList()
        };

    public static EnergyCostComponentDto ToDto(this EnergyCostComponent component)
        => new()
        {
            Id = component.Id,
            EnergyId = component.EnergyId,
            Key = component.Key,
            Name = component.Name,
            Category = component.Category.ToString(),
            ValueType = component.ValueType.ToString(),
            Value = component.Value,
            Order = component.Order,
            IsEditable = component.IsEditable,
            Notes = component.Notes
        };

    public static SystemParameterDto ToDto(this SystemParameter parameter)
        => new()
        {
            Id = parameter.Id,
            Key = parameter.Key,
            Name = parameter.Name,
            Category = parameter.Category.ToString(),
            Description = parameter.Description,
            Unit = parameter.Unit,
            Value = parameter.Value,
            IsEditable = parameter.IsEditable
        };

    public static Energy ToEntity(this EnergyDto dto, Energy? existing = null)
    {
        var mode = Enum.Parse<EnergyMode>(dto.Mode, true);
        var family = Enum.Parse<EnergyFamily>(dto.Family, true);

        var entity = existing ?? new Energy { Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id };

        entity.Code = dto.Code;
        entity.Name = dto.Name;
        entity.Mode = mode;
        entity.Family = family;
        entity.PricePerUnit = dto.PricePerUnit;
        entity.ConsumptionPer100Km = dto.ConsumptionPer100Km;
        entity.RentingCostPerMonth = dto.RentingCostPerMonth;
        entity.EmissionFactorPerUnit = dto.EmissionFactorPerUnit;
        entity.RenewableShare = dto.RenewableShare;
        entity.EmissionReduction = dto.EmissionReduction;
        entity.BaseEnergyId = dto.BaseEnergyId;
        entity.EmissionReferenceEnergyId = dto.EmissionReferenceEnergyId;
        entity.InheritEmissionFromBase = dto.InheritEmissionFromBase;
        entity.IsActive = dto.IsActive;

        return entity;
    }

    public static EnergyCostComponent ToEntity(this EnergyCostComponentDto dto, EnergyCostComponent? existing = null)
    {
        var category = Enum.Parse<CostComponentCategory>(dto.Category, true);
        var valueType = Enum.Parse<CostComponentValueType>(dto.ValueType, true);

        var entity = existing ?? new EnergyCostComponent { Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id };
        entity.EnergyId = dto.EnergyId;
        entity.Key = string.IsNullOrWhiteSpace(dto.Key) ? GenerateKeyFromName(dto.Name) : dto.Key;
        entity.Name = dto.Name;
        entity.Category = category;
        entity.ValueType = valueType;
        entity.Value = dto.Value;
        entity.Order = dto.Order;
        entity.IsEditable = dto.IsEditable;
        entity.Notes = dto.Notes;

        return entity;
    }

    private static string GenerateKeyFromName(string name)
    {
        var sanitized = name.ToLowerInvariant();
        var chars = sanitized.Select(ch => char.IsLetterOrDigit(ch) ? ch : '.').ToArray();
        return new string(chars).Trim('.');
    }
}
