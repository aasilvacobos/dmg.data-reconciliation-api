using DMG.ProviderInvoicing.BL.Utility;
using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain.Rule;

public static class PaymentTermsRule
{
    private const uint PaymentDueNetDaysDefault = 55;
    private const uint IdentifierNet55Value = 7u;
    public static readonly PaymentTermsIdentifier IdentifierNet55 = new (IdentifierNet55Value);
    private const uint IdentifierPaidByCreditCardValue = 8u;
    public static readonly PaymentTermsIdentifier IdentifierCreditCard = new (IdentifierPaidByCreditCardValue);

    /// Determine the net days due for payment terms
    public static uint ToPaymentDueNetDays(PaymentTermsIdentifier paymentTermsIdentifier) =>
        paymentTermsIdentifier.Value switch
        {
            4u                              => 0,                            // Due on receipt
            IdentifierPaidByCreditCardValue => 0,                            // Paid by credit card
            9u                              => 10,                           // Net 10
            1u                              => 15,                           // Net 15
            10u                             => 20,                           // Net 20
            2u                              => 30,                           // Net 30
            5u                              => 30,                           // 1% Net 30
            6u                              => 30,                           // 2% Net 30
            IdentifierNet55Value            => PaymentDueNetDaysDefault,     // Net 55
            3u                              => 60,                           // Net 60                           
            11u                             => 120,                          // Net 120
            _ => PaymentDueNetDaysDefault                                    // Net 55
        };

    /// Create user-readable text representation of payment terms
    public static NonEmptyText ToText(PaymentTermsIdentifier paymentTermsIdentifier) =>
        paymentTermsIdentifier.Value switch
        {
            4u                              => NonEmptyText.NewUnsafe("Due on receipt"),
            IdentifierPaidByCreditCardValue => NonEmptyText.NewUnsafe("Credit card"),
            _                               => NonEmptyText.NewUnsafe($"Net {ToPaymentDueNetDays(paymentTermsIdentifier)}")
        };
    
    public static bool ToPrePaymentFlag(PaymentTermsIdentifier paymentTermsIdentifier) =>
        paymentTermsIdentifier.Value switch
        {
            4u                              => true,                           // Due on receipt
            IdentifierPaidByCreditCardValue => true,                           // Paid by credit card
            _ => (ToPaymentDueNetDays(paymentTermsIdentifier) < PaymentDueNetDaysDefault) // Less than 55 days is true
        };     
}