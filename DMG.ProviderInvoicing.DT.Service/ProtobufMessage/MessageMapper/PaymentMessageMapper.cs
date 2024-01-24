using DMG.Common;
using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Domain;
using DMG.ProviderInvoicing.DT.Domain.Rule;
using LanguageExt;
using static LanguageExt.Prelude;
using static DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper.MessageMapperUtility;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;

/// Maps payment message to entities. Note the payment message is from the DMG.Common namespace, not Work or JobBilling. 
public static class PaymentMessageMapper
{
    public static JobBillingPayment ToEntityJobBillingPayment(DMG.Common.PaymentDetail messageNullable) =>
        Optional(messageNullable)
            .Match(
                Some:message =>
                    new JobBillingPayment(
                        new (CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(message.TotalAmountPaid))),
                        NonEmptyText.NewOptionUnvalidated(message.PaymentTerms),
                        message.Payments.Freeze().Map(ToJobBillingPaymentTransaction)),
                None:new JobBillingPayment(new(0.0M), Option<NonEmptyText>.None, Lst<JobBillingPaymentTransaction>.Empty));

    public static JobBillingPaymentTransaction ToJobBillingPaymentTransaction(DMG.Common.Payment? messageNullable) =>
        Optional(messageNullable)
            .Match(
                Some: message =>
                    new JobBillingPaymentTransaction(
                        new PaymentAmount(CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(message.AmountPaid))),
                            ToIJobBillingPaymentMethod(message)),
                None: 
                    new JobBillingPaymentTransaction(
                        new PaymentAmount(0),
                        new JobBillingPaymentMethodElectronicFundTransfer(
                            DefaultRequiredDateTimeOffsetValueIfMissing,
                            Option<NonEmptyText>.None)));

    private static IJobBillingPaymentMethod ToIJobBillingPaymentMethod(DMG.Common.Payment message) =>
        message.PaymentMethodCase switch
        {
            Payment.PaymentMethodOneofCase.CreditCardPayment =>
                new JobBillingPaymentMethodCreditCard(
                    ToCreditCardProvider(message.CreditCardPayment.CardProvider),
                    ToDateTimeOffsetDefaultToMinimumDate(message.CreditCardPayment.PaidAt),
                    NonEmptyText.NewOptionUnvalidated(message.CreditCardPayment.Last4Digits),
                    NonEmptyText.NewOptionUnvalidated(message.CreditCardPayment.TransactionReferenceCode)),
            Payment.PaymentMethodOneofCase.EftPayment =>
                new JobBillingPaymentMethodElectronicFundTransfer(
                    ToDateTimeOffsetDefaultToMinimumDate(message.EftPayment.PaidAt),
                    NonEmptyText.NewOptionUnvalidated(message.EftPayment.TransactionReferenceCode)),
            // per Deepak, the Payment.PaymentMethodOneofCase.AccordingToTerms is invalid and will be removed
            _ => // default to empty EFT payment method
                new JobBillingPaymentMethodElectronicFundTransfer(
                    DefaultRequiredDateTimeOffsetValueIfMissing,
                    Option<NonEmptyText>.None),
        };

    public static DT.Domain.CreditCardProvider ToCreditCardProvider (DMG.Common.CreditCardProvider creditCardProvider) =>
        creditCardProvider switch
        {
            Common.CreditCardProvider.Unspecified => Domain.CreditCardProvider.Unspecified,
            Common.CreditCardProvider.CorporateCard => Domain.CreditCardProvider.CorporateCard,
            Common.CreditCardProvider.Stripe => Domain.CreditCardProvider.Stripe,
            Common.CreditCardProvider.PncBank => Domain.CreditCardProvider.PncBank,
            _ => Domain.CreditCardProvider.Unspecified
        };
}