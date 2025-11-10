using System.Text.Json.Serialization;

namespace CalculadoraCostes.Contracts.Calculator;

public class CalculationRequestDto
{
    public decimal? KmsPerDay { get; set; }

    public decimal? DaysPerMonth { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TrailerTypeDto TrailerType { get; set; } = TrailerTypeDto.Trailer;

    public decimal? MarginOverride { get; set; }

    public decimal? PricePerTonCo2Override { get; set; }
}
