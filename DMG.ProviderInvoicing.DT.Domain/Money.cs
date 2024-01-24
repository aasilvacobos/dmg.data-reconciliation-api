using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMG.ProviderInvoicing.DT.Domain;
public class Money
{
    private long _amount { get; }

    public string CurrencyCode { get; }
    public decimal AmountAsDecimal => (decimal)(_amount / 1000m);
    public long AmountAsLong => _amount;

    public Money(string currencyCode, decimal amount)
    {
        CurrencyCode = currencyCode;
        _amount = (long)(amount * 1000);
    }

    public Money(string currencyCode, long amount)
    {
        CurrencyCode = currencyCode;
        _amount = amount;
    }
}
