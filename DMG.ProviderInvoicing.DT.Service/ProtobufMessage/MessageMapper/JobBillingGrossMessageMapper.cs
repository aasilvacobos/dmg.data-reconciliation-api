using System.ComponentModel;
using DMG.Common;
using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Domain;
using DMG.ProviderInvoicing.DT.Domain.Rule;
using Dmg.Work.Billing.V1;
using LanguageExt;
using static LanguageExt.Prelude;
using static DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper.MessageMapperUtility;
using TechnicianType = DMG.ProviderInvoicing.DT.Domain.TechnicianType;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;

/// Provides deterministic mapping between job billing protobuf messages and domain entities. It provides shared functionality
/// among any I/O adapter working with protobuf messages.
public static class JobBillingGrossMessageMapper
{
    private static readonly Lst<string> ExcludedCatalogItemRuleKeys = List("Catalog Item ID", "Material Name", "Part Name", "Equipment Name");
    
    /// Function to build an empty job billing gross material/equipment item.
    private static IJobBillingGrossMaterialPartEquipmentItem BuildEmptyJobBillingGrossMaterialPartEquipmentItem() =>
        new JobBillingGrossMaterialPartEquipmentCatalogItem(new CatalogItemId(Guid.Empty), Lst<JobBillingCatalogItemRuleValue>.Empty);

    /// Map m/p item message to m/p/e item entity on job billing gross
    public static IJobBillingGrossMaterialPartEquipmentItem ToEntityJobBillingGrossMaterialPartItem(Dmg.Work.Billing.V1.MaterialAndPartLineItem? lineItemMessage) =>
        Optional(lineItemMessage)
            .Match(x => x.ItemDetailsCase switch
                        {
                            MaterialAndPartLineItem.ItemDetailsOneofCase.CatalogItemId =>
                                new JobBillingGrossMaterialPartEquipmentCatalogItem(
                                    new CatalogItemId(ParseGuidStringDefaultToEmptyGuid(x.CatalogItemId)),
                                    x.CatalogItemRuleValues.Map(ToJobBillingCatalogItemRuleValue).Freeze()),
                            MaterialAndPartLineItem.ItemDetailsOneofCase.NonCatalogItem =>
                                new JobBillingGrossMaterialPartEquipmentNonCatalogItem(
                                    Optional(x.NonCatalogItem)
                                        .Match(y => y.Name.DefaultIfNullOrWhiteSpace(DefaultRequiredStringValueIfMissing),
                                               () => DefaultRequiredStringValueIfMissing)),
                            _ => new JobBillingGrossMaterialPartEquipmentNonCatalogItem(DefaultRequiredStringValueIfMissing)
                        },
                    BuildEmptyJobBillingGrossMaterialPartEquipmentItem);

    /// Map equipment item message to m/p/e item entity on job billing gross
    public static IJobBillingGrossMaterialPartEquipmentItem ToEntityJobBillingGrossEquipmentItem(Dmg.Work.Billing.V1.EquipmentLineItem? lineItemMessage) =>
        Optional(lineItemMessage)
            .Match(x => x.ItemDetailsCase switch
                        {
                            EquipmentLineItem.ItemDetailsOneofCase.CatalogItemId =>
                                new JobBillingGrossMaterialPartEquipmentCatalogItem(
                                    new CatalogItemId(ParseGuidStringDefaultToEmptyGuid(x.CatalogItemId)),
                                    x.CatalogItemRuleValues.Map(ToJobBillingCatalogItemRuleValue).Freeze()),
                            EquipmentLineItem.ItemDetailsOneofCase.NonCatalogItem =>
                                new JobBillingGrossMaterialPartEquipmentNonCatalogItem(
                                    Optional(x.NonCatalogItem)
                                        .Match(y => y.Name.DefaultIfNullOrWhiteSpace(DefaultRequiredStringValueIfMissing),
                                               () => DefaultRequiredStringValueIfMissing)),
                            _ => new JobBillingGrossMaterialPartEquipmentNonCatalogItem(DefaultRequiredStringValueIfMissing)
                        },
                   BuildEmptyJobBillingGrossMaterialPartEquipmentItem);

    public static MaterialPartEquipmentCatalogItemType ToMaterialPartEquipmentCatalogItemType(DMG.Common.CatalogItemType catalogItemType, MaterialPartEquipmentCatalogItemType defaultMaterialPartEquipmentCatalogItemType) =>
        catalogItemType
            switch
        {
            DMG.Common.CatalogItemType.Material => MaterialPartEquipmentCatalogItemType.Material,
            DMG.Common.CatalogItemType.Part => MaterialPartEquipmentCatalogItemType.Part,
            DMG.Common.CatalogItemType.OwnedEquipment => MaterialPartEquipmentCatalogItemType.EquipmentOwned,
            DMG.Common.CatalogItemType.RentalEquipment => MaterialPartEquipmentCatalogItemType.EquipmentRental,
            _ => defaultMaterialPartEquipmentCatalogItemType
        };

    public static DT.Domain.JobBillingElementCreationSource ToEntityJobBillingElementCreationSource(Source source) =>
        source
            switch
            {
                Dmg.Work.Billing.V1.Source.Operations => JobBillingElementCreationSource.Operations,
                Dmg.Work.Billing.V1.Source.Provider => JobBillingElementCreationSource.Provider,
                Dmg.Work.Billing.V1.Source.System => JobBillingElementCreationSource.System,
                Dmg.Work.Billing.V1.Source.Technician => JobBillingElementCreationSource.Technician,
                Dmg.Work.Billing.V1.Source.JobCosting => JobBillingElementCreationSource.JobCosting,
                Dmg.Work.Billing.V1.Source.Unspecified => JobBillingElementCreationSource.Unspecified,
                _ => JobBillingElementCreationSource.Unspecified
            };
    
    private static Domain.JobBillingCatalogItemEffectiveRule ToJobBillingCatalogItemEffectiveRule(Dmg.Work.Billing.V1.CatalogItemRules message) =>
        new (new CatalogItemId(ParseGuidStringDefaultToEmptyGuid(message.CatalogItemId)),
            Optional(message.CatalogItemRules_)
                .Map(m => ToMapImmutable(m)
                    .Filter((key, val) => !ExcludedCatalogItemRuleKeys.Contains(key) && val.IsSome)));
    
    /// Function to build an empty job billing gross material/equipment line item.
    private static JobBillingGrossMaterialPartEquipmentLineItem BuildEmptyJobBillingGrossMaterialPartEquipmentLineItem(MaterialPartEquipmentCatalogItemType catalogItemType) =>
        new(Guid.Empty, 0,  UnitOfMeasure.Item, 0, new LineItemCost(0.0M), catalogItemType, BuildEmptyJobBillingGrossMaterialPartEquipmentItem(), JobBillingElementCreationSource.Unspecified, 
            Option<JobWorkId>.Some(new (Guid.Empty)), Option<NonEmptyText>.None, RecordMetaMessageMapper.BuildRecordMetaEmpty(), Option<JobBillingElementDispute>.None, Lst<JobBillingRuleMessage>.Empty);

    private static JobBillingCatalogItemRuleValue ToJobBillingCatalogItemRuleValue(Dmg.Work.Billing.V1.CatalogItemRuleValue message) =>
        new (NonEmptyText.NewUnsafe(message.CatalogItemRuleRateType),
            NonEmptyText.NewOptionUnvalidated(message.Value));

    /// Map m/p/e line item message to m/p/e line item entity on job billing gross
    public static DT.Domain.JobBillingGrossMaterialPartEquipmentLineItem ToEntityJobBillingGrossMaterialPartLineItem(Dmg.Work.Billing.V1.MaterialAndPartLineItem? lineItemMessage, JobWorkId jobWorkId) =>
        Optional(lineItemMessage)
            .Match(x => new JobBillingGrossMaterialPartEquipmentLineItem(ParseGuidStringDefaultToEmptyGuid(x.MaterialAndPartLineItemId),
                            QuantityMessageMapper.ToDecimal(x.Quantity),
                            Optional(x.Quantity)
                                .Map(quantity => quantity.UnitType)
                                .Match(CatalogItemMessageMapper.ToUnitOfMeasure, () => UnitOfMeasure.Unspecified),
                            CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(x.Rate)),
                            new LineItemCost(CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(x.TotalAmount))),
                            ToMaterialPartEquipmentCatalogItemType(x.ItemType, MaterialPartEquipmentCatalogItemType.Material),
                            ToEntityJobBillingGrossMaterialPartItem(x),
                            ToEntityJobBillingElementCreationSource(x.LineItemSource),
                            Option<JobWorkId>.Some(jobWorkId),
                            NonEmptyText.NewOptionUnvalidated(x.Reason),
                            RecordMetaMessageMapper.ToEntity(x.MetaData),
                            JobBillingGrossDisputeMessageMapper.ToEntityOption(x.Dispute), 
                            ToJobBillingRuleMessages(x.Messages.Freeze())),
            () => BuildEmptyJobBillingGrossMaterialPartEquipmentLineItem(MaterialPartEquipmentCatalogItemType.Material));

    /// Map equipment line item message to m/p/e line item entity on job billing gross
    public static DT.Domain.JobBillingGrossMaterialPartEquipmentLineItem ToEntityJobBillingGrossEquipmentLineItem(Dmg.Work.Billing.V1.EquipmentLineItem? lineItemMessage, JobWorkId jobWorkId) =>
        Optional(lineItemMessage)
            .Match(x => new JobBillingGrossMaterialPartEquipmentLineItem(ParseGuidStringDefaultToEmptyGuid(x.EquipmentLineItemId),
                            QuantityMessageMapper.ToDecimal(x.Quantity),
                            Optional(x.Quantity)
                                .Map(quantity => quantity.UnitType)
                                .Match(CatalogItemMessageMapper.ToUnitOfMeasure, () => UnitOfMeasure.Unspecified),
                            CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(x.Rate)),
                            new LineItemCost(CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(x.TotalAmount))),
                            ToMaterialPartEquipmentCatalogItemType(x.ItemType, MaterialPartEquipmentCatalogItemType.EquipmentOwned),
                            ToEntityJobBillingGrossEquipmentItem(x),
                            ToEntityJobBillingElementCreationSource(x.LineItemSource),
                            Option<JobWorkId>.Some(jobWorkId),
                            NonEmptyText.NewOptionUnvalidated(x.Reason),
                            RecordMetaMessageMapper.ToEntity(x.MetaData),
                            JobBillingGrossDisputeMessageMapper.ToEntityOption(x.Dispute),
                            ToJobBillingRuleMessages(x.Messages.Freeze())),
            () => BuildEmptyJobBillingGrossMaterialPartEquipmentLineItem(MaterialPartEquipmentCatalogItemType.EquipmentOwned));

    /// Intermediate container to bridge message and entity
    private record CheckInOutDateTime(
        DateTimeOffset                                  CheckInDateTime,
        DateTimeOffset                                  CheckOutDateTime,
        JobBillingGrossTechnicianTripTimeOnSiteSource   TimeOnSiteSource);

    private static CheckInOutDateTime BuildCheckInOutDateTimeEmpty() =>
        new(DefaultRequiredDateTimeOffsetValueIfMissing,
            DefaultRequiredDateTimeOffsetValueIfMissing,
            JobBillingGrossTechnicianTripTimeOnSiteSource.TimeOnSiteSourceTechnician);

    private static CheckInOutDateTime ToCheckInOutDateTime(TimeOnSite? timeOnSite) =>
        Optional(timeOnSite)
            .Match(x => x.TimeOnSiteCase switch
            {
                TimeOnSite.TimeOnSiteOneofCase.ManualTos =>
                    new CheckInOutDateTime(
                        ToDateTimeOffsetDefaultToMinimumDate(x.ManualTos.ArrivalTime),
                        ToDateTimeOffsetDefaultToMinimumDate(x.ManualTos.DepartureTime),
                        JobBillingGrossTechnicianTripTimeOnSiteSource.TimeOnSiteSourceTechnician),
                TimeOnSite.TimeOnSiteOneofCase.SystemTos =>
                    new CheckInOutDateTime(
                        ToDateTimeOffsetDefaultToMinimumDate(x.SystemTos.CheckInRecord.TimeUtc),
                        ToDateTimeOffsetDefaultToMinimumDate(x.SystemTos.CheckOutRecord.TimeUtc),
                        JobBillingGrossTechnicianTripTimeOnSiteSource.TimeOnSiteSourceBackOffice),
                _ =>
                    BuildCheckInOutDateTimeEmpty()
            },
                BuildCheckInOutDateTimeEmpty);

    /// Map money adjustment message to cost adjustment for job billing gross
    public static JobBillingGrossCostDiscountLineItem ToEntityJobBillingGrossCostDiscountLineItem(Dmg.Work.Billing.V1.MoneyAdjustment? moneyAdjustmentMessage) =>
        Optional(moneyAdjustmentMessage)
            .Match(x => new JobBillingGrossCostDiscountLineItem(
                            CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(x.AdjustedValue)),
                            ToEntityJobBillingElementCreationSource(x.AdjustmentSource),
                            RecordMetaMessageMapper.ToEntity(x.MetaData)),
                   () => new JobBillingGrossCostDiscountLineItem(0.0M, JobBillingElementCreationSource.Unspecified, RecordMetaMessageMapper.BuildRecordMetaEmpty()));

    /// Map money adjustment message to cost adjustment for job billing
    public static IJobBillingCostDiscountLineItem ToEntityJobBillingCostDiscountLineItem(Dmg.Work.Billing.V1.MoneyAdjustment? moneyAdjustmentMessage) =>
        Optional(moneyAdjustmentMessage)
            .Match(x => new JobBillingCostDiscountLineItem(
                    CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(x.AdjustedValue)),
                    ToEntityJobBillingElementCreationSource(x.AdjustmentSource),
                    RecordMetaMessageMapper.ToEntity(x.MetaData)),
                () => new JobBillingCostDiscountLineItem(0.0M, JobBillingElementCreationSource.Unspecified, RecordMetaMessageMapper.BuildRecordMetaEmpty()));
    
    /// Map time adjustment message to time adjustment trip entity for job billing gross
    public static JobBillingGrossLaborLineItemTimeAdjustment ToEntityJobBillingGrossLaborLineItemTimeAdjustment(Dmg.Work.Billing.V1.TimeAdjustment? timeAdjustmentMessage) =>
        Optional(timeAdjustmentMessage)
            .Match(
                x => new JobBillingGrossLaborLineItemTimeAdjustment(
                    new AdjustmentSeconds(x.Seconds),
                    new TimeAdjustmentCost(CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(x.ExtendedAmount))),
                    Optional(x.HasProviderConfirmed).IfNone(false),
                    ToEntityJobBillingElementCreationSource(x.AdjustmentSource),
                    NonEmptyText.NewOptionUnvalidated(x.Reason),
                    RecordMetaMessageMapper.ToEntity(x.MetaData),
                    ToJobBillingRuleMessages(x.Messages.Freeze())),
                () => new JobBillingGrossLaborLineItemTimeAdjustment(
                    new AdjustmentSeconds(0),
                    new TimeAdjustmentCost(0.0M),
                    false,
                    JobBillingElementCreationSource.Unspecified,
                    Option<NonEmptyText>.None,
                    RecordMetaMessageMapper.BuildRecordMetaEmpty(),
                    Lst<JobBillingRuleMessage>.Empty) );

    /// Map technician trip message to technician trip entity for job billing gross
    public static JobBillingGrossTechnicianTrip ToEntityJobBillingGrossTechnicianTrip(Dmg.Work.Billing.V1.TechnicianLabor? jobTechnicianTripMessage) =>
        Optional(jobTechnicianTripMessage)
            .Match(x =>
            {
                CheckInOutDateTime checkInOutDateTime = ToCheckInOutDateTime(x.TimeOnsiteDetails);

                return new JobBillingGrossTechnicianTrip(
                    new TechnicianTripId(ParseGuidStringDefaultToEmptyGuid(x.TechnicianLaborLineId)),
                    new UserId(ParseGuidStringDefaultToEmptyGuid(x.TechnicianUserId)),
                    checkInOutDateTime.CheckInDateTime,
                    checkInOutDateTime.CheckOutDateTime,
                    x.TotalBillableTimeSeconds,
                    x.IsPayable,
                    checkInOutDateTime.TimeOnSiteSource,
                    ToEntityJobBillingElementCreationSource(x.LineItemSource));
            },
            () => new JobBillingGrossTechnicianTrip(
                new TechnicianTripId(Guid.Empty),
                new UserId(Guid.Empty),
                DefaultRequiredDateTimeOffsetValueIfMissing,
                DefaultRequiredDateTimeOffsetValueIfMissing,
                0,
                false,
                JobBillingGrossTechnicianTripTimeOnSiteSource.TimeOnSiteSourceTechnician,
                JobBillingElementCreationSource.Unspecified));

    /// Function to convert a message type to a <see cref="JobBillingMessageType"/> type.
    public static JobBillingMessageType ToJobBillingMessageType(Dmg.Work.Billing.V1.MessageType messageType)
        => messageType switch
        {
            Dmg.Work.Billing.V1.MessageType.Note => JobBillingMessageType.Note,
            Dmg.Work.Billing.V1.MessageType.Warning => JobBillingMessageType.Warning,
            Dmg.Work.Billing.V1.MessageType.Alert => JobBillingMessageType.Alert,
            _ => JobBillingMessageType.Note // TODO move default case to domain, message mapper should be agnostic
        };

    public static DT.Domain.TechnicianType ToJobBillingTechnicianType(Dmg.Work.V1.TechnicianType messageType)
        => messageType switch
        {
            Dmg.Work.V1.TechnicianType.Regular => TechnicianType.TechnicianTypeRegular,
            Dmg.Work.V1.TechnicianType.Helper => TechnicianType.TechnicianTypeHelper,
            Dmg.Work.V1.TechnicianType.Unknown => JobBillingRule.GetTechnicianTypeDefault(),
            _ => JobBillingRule.GetTechnicianTypeDefault()
        };

    
    /// Function to convert a visibility type to a <see cref="JobBillingMessageVisibility"/> type.
    public static JobBillingMessageVisibility ToJobBillingMessageVisibility(Dmg.Work.Billing.V1.Visibility visibility)
        => visibility switch
        {
            Dmg.Work.Billing.V1.Visibility.Provider => JobBillingMessageVisibility.Provider,
            Dmg.Work.Billing.V1.Visibility.Operations => JobBillingMessageVisibility.Operations,
            // a missing visibility should default to not visible to the Provider
            _ => JobBillingMessageVisibility.Operations // TODO move default case to domain, message mapper should be agnostic
        };

    public static JobBillingSubmittedBySource ToJobBillingSubmittedBySource(Dmg.Work.Billing.V1.SubmittedBy message)
        => message switch
        {
            Dmg.Work.Billing.V1.SubmittedBy.InternalApi => JobBillingSubmittedBySource.InternalApi,
            Dmg.Work.Billing.V1.SubmittedBy.ProviderInvoicingSystemAction => JobBillingSubmittedBySource.AutoSubmitSystemAction,
            Dmg.Work.Billing.V1.SubmittedBy.Operations => JobBillingSubmittedBySource.Operations,
            Dmg.Work.Billing.V1.SubmittedBy.ProviderInvoicingUserAction => JobBillingSubmittedBySource.ProviderUserAction,
            Dmg.Work.Billing.V1.SubmittedBy.TakeControlTaskAction => JobBillingSubmittedBySource.TakeControlTaskAction,
            Dmg.Work.Billing.V1.SubmittedBy.ComplaintJobInvoicingSystemAction => JobBillingSubmittedBySource.ComplaintJobInvoicingSystemAction,
            _ => JobBillingSubmittedBySource.Unspecified
        };
    
    /// Function to convert a billing rule type to a <see cref="JobBillingMessageRule"/> type.
    public static JobBillingMessageRule ToJobBillingMessageRule(Dmg.Work.Billing.V1.BillingRule billingRule)
        => billingRule switch
        {
            Dmg.Work.Billing.V1.BillingRule.RemoveMinCheckInCheckOutLabor => JobBillingMessageRule.RemoveMinCheckInCheckOutLabor,
            Dmg.Work.Billing.V1.BillingRule.RoundUpToNextMinLabor => JobBillingMessageRule.RoundUpToNextMinLabor,
            Dmg.Work.Billing.V1.BillingRule.QtyOfLaborForThisServiceType => JobBillingMessageRule.QtyOfLaborForThisServiceType,
            Dmg.Work.Billing.V1.BillingRule.LaborTotalCalculation => JobBillingMessageRule.LaborTotalCalculation,
            Dmg.Work.Billing.V1.BillingRule.RemoveExtraTripsForSameDaySameProperty => JobBillingMessageRule.RemoveExtraTripsForSameDaySameProperty,
            Dmg.Work.Billing.V1.BillingRule.RemoveExtraTrips => JobBillingMessageRule.RemoveExtraTrips,
            Dmg.Work.Billing.V1.BillingRule.AddMissedArrivalLineItemBasedOnUrgencyTrip => JobBillingMessageRule.AddMissedArrivalLineItemBasedOnUrgencyTrip,
            Dmg.Work.Billing.V1.BillingRule.TripTotalCalculation => JobBillingMessageRule.TripTotalCalculation,
            Dmg.Work.Billing.V1.BillingRule.RemoveUnPayablePartsAndMaterial => JobBillingMessageRule.RemoveUnPayablePartsAndMaterial,
            Dmg.Work.Billing.V1.BillingRule.CheckVarianceOfQtyUsageForServiceTypePartsAndMaterial => JobBillingMessageRule.CheckVarianceOfQtyUsageForServiceTypePartsAndMaterial,
            Dmg.Work.Billing.V1.BillingRule.CheckForNonCataloguePartsAndMaterial => JobBillingMessageRule.CheckForNonCataloguePartsAndMaterial,
            Dmg.Work.Billing.V1.BillingRule.CheckVarianceForRateComparedToOurStandardsPerServiceTypeForPartsAndMaterial => JobBillingMessageRule.CheckVarianceForRateComparedToOurStandardsPerServiceTypeForPartsAndMaterial,
            Dmg.Work.Billing.V1.BillingRule.PartsAndMaterialTotalCalculation => JobBillingMessageRule.PartsAndMaterialTotalCalculation,
            Dmg.Work.Billing.V1.BillingRule.RemoveUnPayableEquipments => JobBillingMessageRule.RemoveUnPayableEquipments,
            Dmg.Work.Billing.V1.BillingRule.CheckVarianceOfQtyUsageForServiceTypeEquipment => JobBillingMessageRule.CheckVarianceOfQtyUsageForServiceTypeEquipment,
            Dmg.Work.Billing.V1.BillingRule.CheckForNonCatalogueEquipment => JobBillingMessageRule.CheckForNonCatalogueEquipment,
            Dmg.Work.Billing.V1.BillingRule.CheckVarianceForRateComparedToOurStandardsPerServiceTypeEquipment => JobBillingMessageRule.CheckVarianceForRateComparedToOurStandardsPerServiceTypeEquipment,
            Dmg.Work.Billing.V1.BillingRule.EquipmentTotalCalculation => JobBillingMessageRule.EquipmentTotalCalculation,
            Dmg.Work.Billing.V1.BillingRule.TotalAmountCalculation => JobBillingMessageRule.TotalAmountCalculation,
            Dmg.Work.Billing.V1.BillingRule.CheckTotalAmountAgainstNte => JobBillingMessageRule.CheckTotalAmountAgainstNte,
            Dmg.Work.Billing.V1.BillingRule.CheckAverageTotalAmountAgainstPreviousWorkOfSameServiceLine => JobBillingMessageRule.CheckAverageTotalAmountAgainstPreviousWorkOfSameServiceLine,
            Dmg.Work.Billing.V1.BillingRule.CheckPartsAndMaterialCatalogItemRate => JobBillingMessageRule.CheckPartsAndMaterialCatalogItemRate,
            Dmg.Work.Billing.V1.BillingRule.CheckEquipmentCatalogItemRate => JobBillingMessageRule.CheckEquipmentCatalogItemRate,
            Dmg.Work.Billing.V1.BillingRule.FlatRateJobTotalCalculation => JobBillingMessageRule.FlatRateJobTotalCalculation,
            Dmg.Work.Billing.V1.BillingRule.CheckFlatRateJobTotalAmountIsNegative => JobBillingMessageRule.CheckFlatRateJobTotalAmountIsNegative,
            Dmg.Work.Billing.V1.BillingRule.FlatRatePartsAndMaterialTotalCalculation => JobBillingMessageRule.FlatRatePartsAndMaterialTotalCalculation,
            Dmg.Work.Billing.V1.BillingRule.CheckFlatRatePartsAndMaterialTotalAmountIsNegative => JobBillingMessageRule.CheckFlatRatePartsAndMaterialTotalAmountIsNegative,
            Dmg.Work.Billing.V1.BillingRule.FlatRateEquipmentTotalCalculation => JobBillingMessageRule.FlatRateEquipmentTotalCalculation,
            Dmg.Work.Billing.V1.BillingRule.CheckFlatRateEquipmentTotalAmountIsNegative => JobBillingMessageRule.CheckFlatRateEquipmentTotalAmountIsNegative,
            Dmg.Work.Billing.V1.BillingRule.MaxUsageHoursCalculation => JobBillingMessageRule.MaxUsageHoursCalculation,
            Dmg.Work.Billing.V1.BillingRule.CheckIsCreditCardProvider => JobBillingMessageRule.CheckIsCreditCardProvider,
            // Deprecated cases
            //Dmg.Work.Billing.V1.BillingRule.FlagNonCatalogItemsDescriptionPartsAndMaterial => JobBillingMessageRule.FlagNonCatalogItemsDescriptionPartsAndMaterial, 
            //Dmg.Work.Billing.V1.BillingRule.FlagNonCatalogItemsDescriptionEquipment => JobBillingMessageRule.FlagNonCatalogItemsDescriptionEquipment, 
            Dmg.Work.Billing.V1.BillingRule.FlagNonCatalogItemsNamePartsAndMaterial => JobBillingMessageRule.FlagNonCatalogItemsNamePartsAndMaterial, 
            Dmg.Work.Billing.V1.BillingRule.FlagNonCatalogItemsNameEquipment => JobBillingMessageRule.FlagNonCatalogItemsNameEquipment,
            Dmg.Work.Billing.V1.BillingRule.AutoDeductLabor => JobBillingMessageRule.AutoDeductLabor,
            _ => JobBillingMessageRule.Unspecified
        };

    /// Function to convert a message into a <see cref="JobBillingRuleMessage"/> type.
    public static JobBillingRuleMessage ToJobBillingRuleMessage(Dmg.Work.Billing.V1.Message? jobBillingMessageNullable) =>
        Optional(jobBillingMessageNullable)
            .Match(
                x =>
                    new JobBillingRuleMessage(
                        ToJobBillingMessageType(x.MessageType),
                        ToJobBillingMessageRule(x.BillingRule),
                        NonEmptyText.NewOptionUnvalidated(x.Message_),
                        Optional(x.MessageVisibleFor)
                            .Match(
                                y => y.Freeze().Map(ToJobBillingMessageVisibility), 
                                () => Lst<JobBillingMessageVisibility>.Empty)),
                () =>
                    new JobBillingRuleMessage(
                        JobBillingMessageType.Note,
                        JobBillingMessageRule.Unspecified,
                        Option<NonEmptyText>.None,
                        Lst<JobBillingMessageVisibility>.Empty));

    public static Lst<JobBillingRuleMessage> ToJobBillingRuleMessages(Lst<Dmg.Work.Billing.V1.Message> messages) =>
        messages
            .Map(ToJobBillingRuleMessage)
            // this predicate prevents the provider from seeing rule messages intended for only the OC
            .Filter(JobBillingRule.IsRuleMessageVisible);
    
    /// Map labor line message to labor line entity for job billing gross
    private static DT.Domain.JobBillingGrossLaborLineItem ToEntityJobBillingGrossLaborLineItem(Dmg.Work.Billing.V1.LaborLineItem? lineItemMessage, JobWorkId jobWorkId) =>
        Optional(lineItemMessage)
            .Match(x => new JobBillingGrossLaborLineItem(
                    new(ParseGuidStringDefaultToEmptyGuid(x.LaborLineItemId)),
                    jobWorkId,
                    new LaborRate(CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(x.Rate))),
                    CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(x.AdjustedExtendedAmount)),
                    AdjustedTotalBillableTimeInHours:DateTimeUtility.ConvertSecondsToHours(x.AdjustedTotalBillableTimeSeconds),
                    x.TotalBillableTimeSeconds,
                    x.AdjustedTotalBillableTimeSeconds,
                    CostingMessageMapper.ToEntity(x.RateType),
                    ToJobBillingTechnicianType(x.TechnicianType),
                    JobBillingGrossDisputeMessageMapper.ToEntityOption(x.Dispute),
                    x.Labor.Freeze().Select(ToEntityJobBillingGrossTechnicianTrip),
                    x.TimeAdjustment.Freeze().Select(ToEntityJobBillingGrossLaborLineItemTimeAdjustment),
                    ToJobBillingRuleMessages(x.Messages.Freeze())),
                () => new JobBillingGrossLaborLineItem(
                    new(Guid.Empty), 
                    new JobWorkId(Guid.Empty),
                    new LaborRate(0.0M),
                    0, 
                    0, 
                    0, 
                    0,
                    CostingMessageMapper.ToEntity(DMG.Common.RateType.Regular),
                    DT.Domain.TechnicianType.TechnicianTypeRegular,
                    Option<JobBillingElementDispute>.None,
                    Lst<JobBillingGrossTechnicianTrip>.Empty,
                    Lst<JobBillingGrossLaborLineItemTimeAdjustment>.Empty, 
                    Lst<JobBillingRuleMessage>.Empty)); 

    public static DT.Domain.JobBillingGrossTripChargeLineItem ToJobBillingGrossTripChargeLineItem(Dmg.Work.Billing.V1.TripLineItem? tripChargeLineItemMessage) =>
        Optional(tripChargeLineItemMessage)
            .Match(
                x =>
                    new JobBillingGrossTripChargeLineItem(
                        ParseGuidStringDefaultToEmptyGuid(x.TripLineItemId),
                        new(ParseGuidStringDefaultToEmptyGuid(x.WorkVisitId)),
                        CostingMessageMapper.ToEntity(x.RateType),
                        // Trip charge rate and line item cost are the same value since a quantity of 1.0 is assumed; this could change.
                        new TripChargeRate(CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(x.ExtendedAmount))), 
                        new LineItemCost(CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(x.ExtendedAmount))),
                        ToDateTimeOffsetDefaultToMinimumDate(x.FirstTechnicianArrivalTimeUtc),
                        Optional(x.TripSchedule)
                            .Match(
                                y => ToDateTimeOffsetDefaultToMinimumDate(y.EndDateUtc),
                                () => DefaultRequiredDateTimeOffsetValueIfMissing),
                        x.IsTripPayable,
                        ToEntityJobBillingElementCreationSource(x.LineItemSource),
                        Optional(x.Messages)
                            // TODO safer to not use ToJobBillingRuleMessages (Provider visibility only) until JobBillingGrossRule.IsAddMissedArrivalLineItemBasedOnUrgencyTrip is moved to new JB
                            .Match(
                                y => y.Freeze().Map(ToJobBillingRuleMessage),
                                () => Lst<JobBillingRuleMessage>.Empty)),
                () => 
                    new JobBillingGrossTripChargeLineItem(
                        Guid.Empty,
                        new WorkVisitId(Guid.Empty),
                        DT.Domain.RateType.Regular,
                        new TripChargeRate(0.0M),
                        new LineItemCost(0.0M),
                        DefaultRequiredDateTimeOffsetValueIfMissing,
                        DefaultRequiredDateTimeOffsetValueIfMissing,
                        false,
                        JobBillingElementCreationSource.Unspecified,
                        Lst<JobBillingRuleMessage>.Empty));

    private static DT.Domain.JobBillingReviewRequestMessage ToJobBillingReviewRequestMessage(Option<Dmg.Work.Billing.V1.RequestMessage> requestMessageOption) =>
        requestMessageOption
            .Match(
                ToJobBillingReviewRequestMessage,
                () =>
                    new JobBillingReviewRequestMessage(
                        new UserId(Guid.Empty),
                        DefaultRequiredDateTimeOffsetValueIfMissing,
                        NonEmptyText.NewUnsafe(DefaultRequiredStringValueIfMissing)));

    public static JobBillingReviewRequestMessage ToJobBillingReviewRequestMessage(Dmg.Work.Billing.V1.RequestMessage requestMessage) =>
        new JobBillingReviewRequestMessage(new UserId(
                   ParseGuidStringDefaultToEmptyGuid(requestMessage.RequestedByUserId)),
                   ToDateTimeOffsetDefaultToMinimumDate(requestMessage.RequestedOnUtc),
                   NonEmptyText.NewUnsafe(requestMessage.RequestMessage_.DefaultIfNullOrWhiteSpace(DefaultRequiredStringValueIfMissing)));

    private static Option<DT.Domain.JobBillingReviewResponseMessage> ToJobBillingReviewResponseMessage(Option<Dmg.Work.Billing.V1.ResponseMessage> requestMessageOption) =>
        requestMessageOption
            .Match(
                rm => Option<DT.Domain.JobBillingReviewResponseMessage>.Some(
                    new JobBillingReviewResponseMessage(new UserId(
                        ParseGuidStringDefaultToEmptyGuid(rm.RespondedByUserId)),
                        ToDateTimeOffsetDefaultToMinimumDate(rm.RespondedOnUtc),
                        NonEmptyText.NewUnsafe(rm.ResponseMessage_.DefaultIfNullOrWhiteSpace(DefaultRequiredStringValueIfMissing)))),
                () => Option<DT.Domain.JobBillingReviewResponseMessage>.None);

    private static DT.Domain.JobBillingReviewConversation ToJobBillingReviewConversation(Dmg.Work.Billing.V1.ProviderReviewRequestConversation? conversationMessage) =>
        Optional(conversationMessage)
            .Match(
                cm =>
                    new JobBillingReviewConversation(
                        new ReviewConversationId(ParseGuidStringDefaultToEmptyGuid(cm.ProviderReviewRequestConversationId)),
                        cm.Order,
                        ToJobBillingReviewRequestMessage(Optional(cm.RequestMessage)),
                        ToJobBillingReviewResponseMessage(Optional(cm.ResponseMessage)),
                        RecordMetaMessageMapper.ToEntity(cm.MetaData)),
                () =>
                    new JobBillingReviewConversation(
                        new ReviewConversationId(Guid.Empty),
                        0,
                        ToJobBillingReviewRequestMessage(Option<RequestMessage>.None),
                        ToJobBillingReviewResponseMessage(Option<ResponseMessage>.None),
                        RecordMetaMessageMapper.ToEntity(Option<RecordMetaData>.None)));

    private static Option<DT.Domain.JobBillingReview> ToJobBillingGrossReviewRequest(Dmg.Work.Billing.V1.ProviderReview? reviewMessage) =>
        Optional(reviewMessage)
            .Filter(message =>
                TryParseGuidString(message.ProviderReviewRequestId).IsSome
                || !message.ProviderReviewRequestConversations.Freeze().IsEmpty)
            .Map(x => new
                JobBillingReview(
                    new JobBillingReviewId(ParseGuidStringDefaultToEmptyGuid(x.ProviderReviewRequestId)),
                    x.ProviderReviewRequestConversations.Freeze().Map(ToJobBillingReviewConversation)));

    /// Function to convert a SOR job billing status into a <see cref="Domain.JobBillingStatus"/> value.
    public static Domain.JobBillingStatus ToJobBillingStatus(Dmg.Work.Billing.V1.JobBillingStatus status) =>
        status switch
        {
            Dmg.Work.Billing.V1.JobBillingStatus.InProgress => Domain.JobBillingStatus.InProgress,
            Dmg.Work.Billing.V1.JobBillingStatus.Verified => Domain.JobBillingStatus.Verified,
            Dmg.Work.Billing.V1.JobBillingStatus.Todo => Domain.JobBillingStatus.Todo,
            Dmg.Work.Billing.V1.JobBillingStatus.Unspecified => Domain.JobBillingStatus.Unspecified,
            Dmg.Work.Billing.V1.JobBillingStatus.Cancelled => Domain.JobBillingStatus.Canceled,
            _ => Domain.JobBillingStatus.Unspecified
        };

    public static Domain.JobBillingCostingScheme ToJobBillingCostingScheme(DMG.DataServices.RatePreference ratePreference) =>
        ratePreference switch
        {
            DMG.DataServices.RatePreference.TimeAndMaterials => JobBillingCostingScheme.TimeAndMaterial,
            DMG.DataServices.RatePreference.FlatRate => JobBillingCostingScheme.FlatRate,
            DMG.DataServices.RatePreference.ServiceBased => JobBillingCostingScheme.ServiceBased,
            // a missing/unspecified costing scheme should default to T&M
            DMG.DataServices.RatePreference.Unspecified => JobBillingCostingScheme.TimeAndMaterial,
            _ => JobBillingCostingScheme.TimeAndMaterial
        };
    
    public static Domain.JobBillingAssignee ToJobBillingAssignee(Dmg.Work.Billing.V1.AssignedToEntity status) =>
        status switch
        {
            Dmg.Work.Billing.V1.AssignedToEntity.Operations => Domain.JobBillingAssignee.Operations,
            Dmg.Work.Billing.V1.AssignedToEntity.Provider => Domain.JobBillingAssignee.Provider,
            Dmg.Work.Billing.V1.AssignedToEntity.Unspecified => JobBillingRule.GetAssigneeDefault(),
            _ => JobBillingRule.GetAssigneeDefault()
        };

    private static Domain.JobBillingSubmissionDetail ToJobBillingSubmissionDetail(Dmg.Work.Billing.V1.SubmissionDetails message) =>
        new (message.SubmittedOn.ToDateTimeOffset(), ToJobBillingSubmittedBySource(message.SubmittedBy));
    
    private static Domain.JobBillingAdditional ToJobBillingGrossAdditional(Dmg.Work.Billing.V1.AdditionalData message) =>
        new (message.CatalogItemRules.Freeze().Map(ToJobBillingCatalogItemEffectiveRule));
    
    /// Map job billing gross message to job billing gross entity 
    public static DT.Domain.JobBillingGross ToEntity(Dmg.Work.Billing.V1.JobBillingData jobBillingMessage)
    {
        var jobWorkId = new JobWorkId(ParseGuidStringDefaultToEmptyGuid(jobBillingMessage.WorkId));
        var recordMeta = RecordMetaMessageMapper.ToEntity(jobBillingMessage.BillingMetadata);
        var materialPartOption = Optional(jobBillingMessage.PartAndMaterial);
        var equipmentOption = Optional(jobBillingMessage.Equipment);
        var laborOption = Optional(jobBillingMessage.Labor);

        return new JobBillingGross(new JobBillingId(ParseGuidStringDefaultToEmptyGuid(jobBillingMessage.JobBillingId)),
            jobWorkId,
            new JobBillingVersion(Convert.ToUInt32(jobBillingMessage.Version)),
            ToJobBillingAssignee(jobBillingMessage.AssignedTo),
            ToJobBillingCostingScheme(jobBillingMessage.RatePreference),
            CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(jobBillingMessage.ExtendedAmount)),
            recordMeta,
            NonEmptyText.NewOptionUnvalidated(jobBillingMessage.ProviderJobSummary),            
            NonEmptyText.NewOptionUnvalidated(jobBillingMessage.ProviderInvoiceNumber),
            TryToDateTimeOffset(jobBillingMessage.ProviderSubmittedOn),
            TryToDateTimeOffset(jobBillingMessage.ProviderFirstSubmitTimeUtc)
                .BiBind(
                    val => val, 
                    () => TryToDateTimeOffset(jobBillingMessage.ProviderSubmittedOn)), // use legacy field if new field has no data
            TryToDateTimeOffset(jobBillingMessage.LastProviderSubmitTimeUtc),
            new JobBillingGrossLabor(
                CurrencyConverter.ConvertAmountToDollars(MoneyMessageMapper.ToCurrency(Optional(jobBillingMessage.Labor).MatchUnsafe(x => x.ExtendedAmount, () => null))),
                laborOption
                    .Bind(labor => Optional(labor.LaborLineItems))
                    .Map(repeatedField => repeatedField.Freeze().Map(laborLineItem => ToEntityJobBillingGrossLaborLineItem(laborLineItem, jobWorkId)))
                    .Match(lst => lst, Lst<JobBillingGrossLaborLineItem>.Empty),
                laborOption
                    .Bind(labor => Optional(labor.MoneyAdjustment))
                    .Map(repeatedField => repeatedField.Freeze().Map(ToEntityJobBillingGrossCostDiscountLineItem))
                    .Match(lst => lst, Lst<JobBillingGrossCostDiscountLineItem>.Empty),
                laborOption
                    .Map(labor => ToJobBillingRuleMessages(labor.Message.Freeze()))
                    .IfNone(Lst<JobBillingRuleMessage>.Empty)),
            new JobBillingGrossMaterialPart(
                materialPartOption
                    .Bind(materialAndPart => Optional(materialAndPart.MaterialsAndPartsLineItems))
                    .Map(repeatedField => repeatedField.Freeze().Map(materialPartLineItem => ToEntityJobBillingGrossMaterialPartLineItem(materialPartLineItem, jobWorkId)))
                    .Match(lst => lst, Lst<JobBillingGrossMaterialPartEquipmentLineItem>.Empty),
                materialPartOption
                    .Bind(materialAndPart => Optional(materialAndPart.MoneyAdjustment))
                    .Map(repeatedField => repeatedField.Freeze().Map(ToEntityJobBillingGrossCostDiscountLineItem))
                    .Match(lst => lst, Lst<JobBillingGrossCostDiscountLineItem>.Empty),
                materialPartOption
                    .Map(materialAndPart => ToJobBillingRuleMessages(materialAndPart.Message.Freeze()))
                    .IfNone(Lst<JobBillingRuleMessage>.Empty)),
            new JobBillingGrossEquipment(
                equipmentOption
                    .Bind(equipment => Optional(equipment.EquipmentLineItems))
                    .Map(repeatedField => repeatedField.Freeze().Map(equipmentLineItem => ToEntityJobBillingGrossEquipmentLineItem(equipmentLineItem, jobWorkId)))
                    .Match(lst => lst, Lst<JobBillingGrossMaterialPartEquipmentLineItem>.Empty),
                equipmentOption
                    .Bind(equipment => Optional(equipment.MoneyAdjustment))
                    .Map(repeatedField => repeatedField.Freeze().Map(ToEntityJobBillingGrossCostDiscountLineItem))
                    .Match(lst => lst, Lst<JobBillingGrossCostDiscountLineItem>.Empty),
                equipmentOption
                    .Map(equipment => ToJobBillingRuleMessages(equipment.Message.Freeze()))
                    .IfNone(Lst<JobBillingRuleMessage>.Empty)),
            JobBillingFlatRateMessageMapper.ToEntityJobBillingJobFlatRate(jobBillingMessage.FlatRateDetails),   
            JobBillingFlatRateMessageMapper.BuildEmptyJobBillingGrossMaterialPartFlatRate(),
            JobBillingFlatRateMessageMapper.BuildEmptyJobBillingGrossEquipmentFlatRate(),
            PaymentMessageMapper.ToEntityJobBillingPayment(jobBillingMessage.PaymentDetails),
            ToJobBillingStatus(jobBillingMessage.JobBillingStatus),
            Optional(jobBillingMessage.LatestSubmissionDetails).Map(ToJobBillingSubmissionDetail),
            Optional(jobBillingMessage.AdditionalData).Map(ToJobBillingGrossAdditional),
            Optional(jobBillingMessage.TripCharge)
                .Bind(trip => Optional(trip.TripLineItems))
                .Map(repeatedField => repeatedField.Freeze().Map(ToJobBillingGrossTripChargeLineItem))
                .Match(lst => lst, Lst<JobBillingGrossTripChargeLineItem>.Empty),
            ToJobBillingRuleMessages(jobBillingMessage.Message.Freeze()),
            jobBillingMessage.Photos.Freeze()
                .Map(jobPhotoMessage => JobPhotoMessageMapper.ToEntity(jobPhotoMessage, jobWorkId)),
            jobBillingMessage.Documents.Freeze()
                .Map(jobDocumentMessage => JobDocumentMessageMapper.ToEntity(jobDocumentMessage, jobWorkId)));
    }
}