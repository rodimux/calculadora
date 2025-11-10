using System.Collections.Generic;

namespace CalculadoraCostes.Contracts.Calculator;

public class CalculationResponseDto
{
    public List<CalculationResultDto> Results { get; set; } = [];

    public decimal KmsPerDay { get; set; }

    public decimal DaysPerMonth { get; set; }
}
