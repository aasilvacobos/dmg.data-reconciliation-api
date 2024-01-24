using DMG.ProviderInvoicing.BL.Utility;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;

/// Provides deterministic mapping between money/currency protobuf messages and domain entities. It provides shared functionality
/// among any I/O adapter working with protobuf messages.
public static class MoneyMessageMapper 
{
    // Default empty currency value.
    private static readonly Domain.Currency _emptyCurrency = new("Unknown", 0);

    /// <summary>
    /// Function to convert a protobuf rate type to a <see cref="Domain.Currency"/> type.
    /// </summary>
    /// <param name="rateNullable">The rate type to convert.</param>
    /// <returns>The currency type.</returns>
    public static DT.Domain.Currency ToCurrency(Dmg.Work.Billing.V1.Rate? rateNullable) =>
        Optional(rateNullable)
            .Match(moneyMessage => ToCurrency(moneyMessage.Value),
                   () => _emptyCurrency);

    public static DT.Domain.Currency ToCurrency(DMG.Common.Money? moneyMessageNullable) =>
        Optional(moneyMessageNullable)  // should convert any null to None
            .Match(moneyMessage => new Domain.Currency(moneyMessage.CurrencyCode.DefaultIfNullOrWhiteSpace(MessageMapperUtility.DefaultRequiredStringValueIfMissing), 
                                                       moneyMessage.Amount),
                () => _emptyCurrency);
}