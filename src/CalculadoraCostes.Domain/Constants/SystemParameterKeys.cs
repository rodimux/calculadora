namespace CalculadoraCostes.Domain.Constants;

public static class SystemParameterKeys
{
    public const string KmsPerDayDefault = "operation.kmsPerDayDefault";
    public const string DaysPerMonthDefault = "operation.daysPerMonthDefault";
    public const string DriverSalary = "operation.driverSalary";
    public const string Margin = "pricing.margin";
    public const string SecondDriverThreshold = "operation.secondDriverThreshold";
    public const string ExtraDriverFactor = "operation.extraDriverFactor";
    public const string TrailerPrice = "assets.trailerPrice";
    public const string DollyPrice = "assets.dollyPrice";
    public const string DuoConsumptionSaving = "operation.duoConsumptionSaving";
    public const string YardCost = "corridor.yardCost";
    public const string TransportCost = "corridor.transportCost";
    public const string DeliveriesPerMonth = "corridor.deliveriesPerMonth";
    public const string CorridorKmDuo = "corridor.duoKm";
    public const string TripsPerMonth = "corridor.tripsPerMonth";
    public const string TollKmSimple = "corridor.tollKmSimple";
    public const string TollKmDuo = "corridor.tollKmDuo";
    public const string TollPricePerKmSimple = "corridor.tollPricePerKmSimple";
    public const string TollPricePerKmDuo = "corridor.tollPricePerKmDuo";
    public const string PricePerTonCo2 = "emissions.priceTonCo2";
    public const string TariffCorrectionFactor = "pricing.tariffCorrectionFactor";
}
