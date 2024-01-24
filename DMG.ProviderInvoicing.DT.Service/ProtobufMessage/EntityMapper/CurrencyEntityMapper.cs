using DMG.Common;
using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Domain;
using DMG.ProviderInvoicing.DT.Domain.Rule;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.EntityMapper; 

/// Provides deterministic mapping from money/currency entity to Protobuf messages.
public static class CurrencyEntityMapper 
{
    // Default empty currency value.
    private const string DefaultCurrencyCode = "USD"; // TODO what about this?

    public static DMG.Common.Money ToMessage(Currency currency) =>
        new ()
        {
            CurrencyCode = currency.CurrencyCode.DefaultIfNullOrWhiteSpace(DefaultCurrencyCode),
            Amount = currency.Amount
        };
    
    public static DMG.Common.Money ToMessage(decimal currencyAmountDecimal, string currencyCode) =>
        ToMessage(new Currency(
            currencyCode, 
            CurrencyConverter.ConvertDollarsToAmount(currencyAmountDecimal)));
}