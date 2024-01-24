using DMG.Common;
using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Domain;
using DMG.ProviderInvoicing.DT.Domain.Rule;
using Dmg.Work.Billing.V1;
using LanguageExt;
using static LanguageExt.Prelude;
using static DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper.MessageMapperUtility;
using static DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper.JobBillingGrossMessageMapper;
namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;

public static class JobBillingFlatRateMessageMapper
{
    private static JobBillingJobFlatRateLineItem ToEntityJobBillingJobFlatRateLineItem(Dmg.Work.Billing.V1.FlatRateJobLineItem message) =>
        new JobBillingJobFlatRateLineItem(
            new LineItemId(ParseGuidStringDefaultToEmptyGuid(message.FlatRateLineItemId)),
            Quantity:QuantityMessageMapper.ToDecimal(message.Quantity),
            Rate:CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(message.Rate.Value)),
            LineItemCost:CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(message.TotalAmount)),
            IsItemPayable:message.IsItemPayable,
            IsFlaggedBySystem:Optional(message.IsFlaggedBySystem).IfNone(false),
            IsManuallyVerified:Optional(message.IsManuallyVerified).IfNone(false),
            JobBillingGrossMessageMapper.ToEntityJobBillingElementCreationSource(message.LineItemSource),
            NonEmptyText.NewOptionUnvalidated(message.Reason),
            ToJobBillingRuleMessages(message.Messages.Freeze()));

    private static JobBillingJobFlatRate BuildEmptyJobBillingJobFlatRate() =>
        new JobBillingJobFlatRate(0.0M, Lst<JobBillingJobFlatRateLineItem>.Empty, Lst<IJobBillingCostDiscountLineItem>.Empty, Lst<JobBillingRuleMessage>.Empty);

    public static JobBillingJobFlatRate ToEntityJobBillingJobFlatRate(Dmg.Work.Billing.V1.FlatRateDetails? messageNullable) =>
        Optional(messageNullable)
            .Bind(x => Optional(x.FlatRateJob))
            .Match(
                Some: message =>
                    new JobBillingJobFlatRate(
                        CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(message.ExtendedAmount)),
                        message.FlatRateJobLineItem.Freeze().Map(ToEntityJobBillingJobFlatRateLineItem),
                        message.MoneyAdjustment.Freeze().Map(JobBillingGrossMessageMapper.ToEntityJobBillingCostDiscountLineItem),
                        ToJobBillingRuleMessages(message.Message.Freeze())),
                None: BuildEmptyJobBillingJobFlatRate());

    internal static JobBillingGrossMaterialPartFlatRate BuildEmptyJobBillingGrossMaterialPartFlatRate() =>
        new JobBillingGrossMaterialPartFlatRate(
            AdjustedTotalCost: 0.0M,
            LineItems: Lst<JobBillingGrossMaterialPartEquipmentFlatRateLineItem>.Empty,
            Lst<IJobBillingCostDiscountLineItem>.Empty,
            Lst<JobBillingRuleMessage>.Empty);

    public static JobBillingGrossMaterialPartFlatRate ToEntityJobBillingGrossMaterialPartFlatRate(Dmg.Work.Billing.V1.FlatRateDetails? messageNullable) =>
        Optional(messageNullable)
            .Bind(x => Optional(x.FlatRateMaterialPart))
            .Match(
                Some: message => BuildEmptyJobBillingGrossMaterialPartFlatRate(), // TODO
                None: BuildEmptyJobBillingGrossMaterialPartFlatRate());        

    public static JobBillingGrossEquipmentFlatRate BuildEmptyJobBillingGrossEquipmentFlatRate() =>
        new JobBillingGrossEquipmentFlatRate(
            AdjustedTotalCost: 0.0M,
            LineItems: Lst<JobBillingGrossMaterialPartEquipmentFlatRateLineItem>.Empty,
            Lst<IJobBillingCostDiscountLineItem>.Empty,
            Lst<JobBillingRuleMessage>.Empty);
    
    public static JobBillingGrossEquipmentFlatRate ToEntityJobBillingGrossEquipmentFlatRate(Dmg.Work.Billing.V1.FlatRateDetails? messageNullable) =>
        Optional(messageNullable)
            .Bind(x => Optional(x.FlatRateEquipment))
            .Match(
                Some: message => BuildEmptyJobBillingGrossEquipmentFlatRate(), // TODO
                None: BuildEmptyJobBillingGrossEquipmentFlatRate());
}