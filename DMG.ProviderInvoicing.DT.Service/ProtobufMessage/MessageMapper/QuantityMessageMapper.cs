using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;

/// Provides deterministic mapping between quantity protobuf messages and the numeric types. 
public static class QuantityMessageMapper
{
    /// Function to map a protobuf the Quantity.Value double to a .NET decimal. Can throw on overflow.
    /// <exception cref="System.OverflowException">Thrown when double value too small or large for decimal.</exception>
    public static decimal ToDecimal(Dmg.Work.Billing.V1.Quantity? quantityNullable) =>
        Optional(quantityNullable)
            .Match(x => Convert.ToDecimal(x.Value),
                   () => 0.0M);
}

