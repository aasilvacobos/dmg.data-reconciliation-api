using DMG.Common;
using DMG.ProviderInvoicing.DT.Domain;


namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.EntityMapper;
public static class UnitOfMeasureEntityMapper
{
    public static UnitType ToMessage(UnitOfMeasure unitOfMeasure) =>
       unitOfMeasure
        switch
       {
           UnitOfMeasure.Unspecified => UnitType.Unspecified,
           UnitOfMeasure.Item => UnitType.Item,
           UnitOfMeasure.Case => UnitType.Case,
           UnitOfMeasure.Gallon => UnitType.Gallon,
           UnitOfMeasure.Liter => UnitType.Liter,
           UnitOfMeasure.Pound => UnitType.Pound,
           UnitOfMeasure.Kilo => UnitType.Kilo,
           UnitOfMeasure.Ton => UnitType.Ton,
           UnitOfMeasure.Minute => UnitType.Minute,
           UnitOfMeasure.Hour => UnitType.Hour,
           UnitOfMeasure.Trip => UnitType.Trip,
           UnitOfMeasure.Day => UnitType.Day,
           UnitOfMeasure.Week => UnitType.Week,
           UnitOfMeasure.Yd3 => UnitType.Yd3,
           UnitOfMeasure.Yd2 => UnitType.Yd2,
           UnitOfMeasure.Mile => UnitType.Mile,
           UnitOfMeasure.Meter => UnitType.Meter,
           UnitOfMeasure.Job => UnitType.Job,
           UnitOfMeasure.Foot => UnitType.Foot,
           UnitOfMeasure.Inch => UnitType.Inch,
           UnitOfMeasure.Event => UnitType.Event,
           UnitOfMeasure.Truck => UnitType.Truck,
           UnitOfMeasure.Bag => UnitType.Bag,
           UnitOfMeasure.Service => UnitType.Service,
           UnitOfMeasure.Second => UnitType.Second,
           _ => UnitType.Unspecified
       };

}
