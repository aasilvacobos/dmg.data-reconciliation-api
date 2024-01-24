namespace DMG.ProviderInvoicing.DT.Domain.Rule;

/// <summary>
/// Handles the logic for the currency entity. We are currently disregarding
/// the currency code and assuming it USD for medium term. 
/// </summary>
public static class CurrencyConverter 
{
    /// Convert the raw currency amount to decimal USD dollars value
    public static decimal ConvertAmountToDollars(Currency currency) =>
        // total amount should be shifted left by three decimal places:
        // 123456 => 123.456
        currency.Amount / 1000.0M;
    
    /// Convert dollars currency to raw currency whole number amount
    public static long ConvertDollarsToAmount(decimal dollars) =>
        // dollars should be shifted right by three decimal places:
        // 123.456 => 123456
        Convert.ToInt64(dollars * 1000.0M);
}