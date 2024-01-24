using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Domain.Rule;
using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain.Map; 

/// Functions to map the job billing to other entities
public static class JobBillingMapper 
{
    public static LineProcessingFee CalculateLineProcessingFee(decimal quantity, ProcessingFee processingFee) 
        => new(quantity * processingFee.Value);
    
    public static IProviderInvoiceMaterialPartEquipmentItem ToProviderInvoiceMaterialPartEquipmentItem(IJobBillingMaterialPartEquipmentItem iJobBillingMaterialPartEquipmentItem, Lst<CatalogItem> catalogItems) =>
        iJobBillingMaterialPartEquipmentItem switch 
        {
            JobBillingMaterialPartEquipmentCatalogItem item =>
                // search for catalog item in order to get name
                match(catalogItems.Find(catalogItem => catalogItem.CatalogItemId == item.CatalogItemId),                
                    catalogItem => new ProviderInvoiceMaterialPartEquipmentCatalogItem(catalogItem.CatalogItemId, catalogItem.Name) as IProviderInvoiceMaterialPartEquipmentItem,
                    () => new ProviderInvoiceMaterialPartEquipmentCatalogItem(item.CatalogItemId, NonEmptyText.NewUnsafe(@"Unknown")) as IProviderInvoiceMaterialPartEquipmentItem),
            JobBillingMaterialPartEquipmentNonCatalogItem item => 
                new ProviderInvoiceMaterialPartEquipmentNonCatalogItem(NonEmptyText.NewUnsafe(item.Name.Value)) as IProviderInvoiceMaterialPartEquipmentItem,
            _ => 
                // valid case to throw because if a new class implements interface, we need to know immediately know that it is not implemented here
                throw new ArgumentException($"Invalid IJobBillingMaterialPartEquipmentItem item argument {nameof(iJobBillingMaterialPartEquipmentItem)}.") 
        };
    
    private static ProviderInvoiceMaterialPartLineItem ToProviderInvoiceMaterialPartLineItem(JobBillingMaterialPartLineItem lineItem, Lst<CatalogItem> catalogItems) =>
        new (lineItem.Id,
            lineItem.CatalogItemType,
            ToProviderInvoiceMaterialPartEquipmentItem(lineItem.Item, catalogItems),
            lineItem.Quantity,
            lineItem.ItemCost,
            lineItem.LineItemCost,
            FalItemTypeMapper.ToLineReference(FalItemType.MaterialPart));

    private static ProviderInvoiceEquipmentLineItem ToProviderInvoiceEquipmentLineItem(JobBillingEquipmentLineItem lineItem, Lst<CatalogItem> catalogItems) =>
        new (lineItem.Id,
            lineItem.CatalogItemType,
            ToProviderInvoiceMaterialPartEquipmentItem(lineItem.Item, catalogItems),
            lineItem.Quantity,
            lineItem.ItemCost,
            lineItem.LineItemCost,
            FalItemTypeMapper.ToLineReference(FalItemType.Equipment));

    private static ProviderInvoiceLaborLineItem ToProviderInvoiceLaborLineItem(IJobBillingLaborLineItem laborLineItem) =>
        new (laborLineItem.TechnicianType, 
            laborLineItem.AdjustedTotalBillableTimeInHours, 
            laborLineItem.RateType,
            laborLineItem.LaborRate, 
            new LineItemCost(laborLineItem.LineItemCost),
            laborLineItem.TechnicianType == TechnicianType.TechnicianTypeRegular ? NonEmptyText.NewUnsafe("Regular Labor") : NonEmptyText.NewUnsafe("Helper Labor"),
            FalItemTypeMapper.ToLineReference(FalItemType.Labor));

    private static ProviderInvoiceProcessingFeeLineItem ToProviderInvoiceProcessingFeeLineItem(JobBillingProcessingFeeLineItem processingFee) =>
        // TODO remove hard coding
        new ProviderInvoiceProcessingFeeLineItem(
            1.00M,
            processingFee.ProcessingFee,
            CalculateLineProcessingFee(1.00M, processingFee.ProcessingFee),
            NonEmptyText.NewUnsafe("Processing Fee"),
            FalItemTypeMapper.ToLineReference(FalItemType.ProcessingFee));

    private static ProviderInvoiceTripChargeLineItem ToProviderInvoiceTripChargeLineItem(JobBillingTripChargeLineItem lineItem) =>
        new (lineItem.Description, 
            lineItem.RequestedByDate,
            lineItem.ArrivalDate, 
            lineItem.RateType,
            lineItem.TripChargeRate,
            lineItem.LineItemCost,
            lineItem.IsRequestedByDateMissed,
            FalItemTypeMapper.ToLineReference(FalItemType.Trip));

    public static decimal NormalizeCostDiscount(decimal costDiscount) => -Math.Abs(costDiscount);
    
    /// Function to convert a <see cref="IJobBillingCostDiscountLineItem"/> to a <see cref="ProviderInvoiceCostDiscountLineItem"/>.
    private static ProviderInvoiceCostDiscountLineItem ToProviderInvoiceCostDiscountLineItem(IJobBillingCostDiscountLineItem lineItem, FalItemType falItemType) =>
        new ProviderInvoiceCostDiscountLineItem(
            NormalizeCostDiscount(lineItem.CostDiscount), 
            FalItemTypeMapper.ToLineReference(falItemType), 
            FalItemTypeMapper.ToDiscountLineDescription(falItemType));

    /// Create the cost discounts for job flat rate of job billing
    private static Lst<ProviderInvoiceCostDiscountLineItem> ToProviderInvoiceCostDiscountLineItems(JobBillingJobFlatRate jobBillingJobFlatRate) =>
        jobBillingJobFlatRate.CostDiscounts.Map(x => ToProviderInvoiceCostDiscountLineItem(x, FalItemType.FlatRateJob));
    
    private static ProviderInvoiceJobFlatRateLineItem ToProviderInvoiceJobFlatRateLineItem(JobBillingJobFlatRateLineItem lineItem, ProviderInvoiceBuilderSupplement providerInvoiceBuilderSupplement) =>
        new (lineItem.Id,
            lineItem.Quantity,
            lineItem.Rate,
            lineItem.LineItemCost,
            lineItem.IsItemPayable,
            lineItem.IsFlaggedBySystem,
            lineItem.IsManuallyVerified,
            providerInvoiceBuilderSupplement.ServiceTypeName.IfNone(NonEmptyText.NewUnsafe("Unknown")), // service type name is used for a job flat rate item            
            lineItem.CreationSource,
            FalItemTypeMapper.ToLineReference(FalItemType.FlatRateJob),
            // optionals
            lineItem.Reason);

    private static ProviderInvoicePayment ToProviderInvoicePayment(JobBillingPayment jobBillingPayment) =>
        new ProviderInvoicePayment(
            jobBillingPayment.TotalAmountPaid, 
            jobBillingPayment.IsPaidByCreditCard,
            jobBillingPayment.PaymentTerms);
    
    private static ProviderInvoiceVendorBill ToProviderInvoiceVendorBill(VendorBill vendorBill) =>
        new ProviderInvoiceVendorBill(
            vendorBill.BillStatus,
            vendorBill.PaymentTermsIdentifier,
            vendorBill.PaymentDueDate,
            vendorBill.IsPrepaid);
    
    /// Convert a job billing to a provider invoice for insert
    /// TODO Adjust to use JobBillingDecorated
    public static ProviderInvoiceInsert ToProviderInvoiceInsert(JobBilling jobBilling, ProviderInvoiceBuilderSupplement providerInvoiceBuilderSupplement) =>
        new(new(Guid.NewGuid()),
            jobBilling.JobBillingId,
            jobBilling.ProviderOrgId,
            jobBilling.JobWorkId,
            jobBilling.TicketId,
            jobBilling.CustomerId,
            jobBilling.PropertyId,
            jobBilling.ServiceLineId,
            providerInvoiceBuilderSupplement.ServiceLineName,
            jobBilling.ServiceTypeId,
            jobBilling.JobSummary.IfNone(NonEmptyText.NewUnsafe("Not Entered")),   // a missing job summary at this point should never occur
            jobBilling.TicketNumber,
            jobBilling.JobWorkNumber,
            providerInvoiceBuilderSupplement.ServicingAddressStateName.IfNone(NonEmptyText.NewUnsafe("Unknown")),    // Per Finance, sending "Unknown" to FAL is acceptable for short term data issue 
            TransactionDate: jobBilling.ProviderSubmittedOnDate.IfNone(DateTimeOffset.Now), // ProviderSubmittedOnDate is "optional" only because Fulfillment was not always setting it; should always have value going forward
            // TODO set ProviderInvoiceStatus to "Pending",
            jobBilling.CostingScheme,
            new RecordMeta(
                jobBilling.RecordMeta.CreatedByUserId, 
                Some(DateTimeOffset.Now),  
                Option<UserId>.None, 
                Option<DateTimeOffset>.None),
            DmgInvoiceNumber: providerInvoiceBuilderSupplement.DmgInvoiceNumber,
            // optional scalars
            ProviderInvoiceNumber: jobBilling.ProviderInvoiceNumber,
            // collections
            jobBilling.MaterialPart.LineItems.Map(jobBillingMaterialPartLineItem => ToProviderInvoiceMaterialPartLineItem(jobBillingMaterialPartLineItem.Core, providerInvoiceBuilderSupplement.CatalogItems)),
            jobBilling.Equipment.LineItems.Map(jobBillingEquipmentLineItem => ToProviderInvoiceEquipmentLineItem(jobBillingEquipmentLineItem.Core, providerInvoiceBuilderSupplement.CatalogItems)),
            jobBilling.Labor.LineItems.Map(ToProviderInvoiceLaborLineItem),
            jobBilling.TripCharge.LineItems.Where(x => !x.IsRequestedByDateMissed).Freeze().Map(ToProviderInvoiceTripChargeLineItem),
            jobBilling.JobFlatRate.LineItems.Map(jobBillingFlatRateLineItem => ToProviderInvoiceJobFlatRateLineItem(jobBillingFlatRateLineItem, providerInvoiceBuilderSupplement)),
            jobBilling.ProcessingFee.LineItems.Map(ToProviderInvoiceProcessingFeeLineItem),
            jobBilling.MaterialPart.CostDiscounts.Map(x => ToProviderInvoiceCostDiscountLineItem(x, FalItemType.MaterialPart)),
            jobBilling.Equipment.CostDiscounts.Map(x => ToProviderInvoiceCostDiscountLineItem(x, FalItemType.Equipment)),
            jobBilling.Labor.CostDiscounts.Map(x => ToProviderInvoiceCostDiscountLineItem(x, FalItemType.Labor)),
            ToProviderInvoiceCostDiscountLineItems(jobBilling.JobFlatRate),
            ToProviderInvoicePayment(jobBilling.Payment));

    /// Create an existing provider invoice from its respective job billing. This mapper is only necessary until we are able to persist the provider invoice
    /// as a provider invoice (distinct from the vendor bill). At that point the provider invoice will be available for retrieval and this method will become obsolete.
    /// This method accepts a vendor bill parameter because the vendor bill must be retrieved first in the temporary approach we are using. In the future, this parameter
    /// should be removed since the vendor bill will be retrieved after the provider invoice is retrieved.
    public static ProviderInvoice ToProviderInvoice(JobBillingDecorated jobBillingDecorated, ProviderInvoiceBuilderSupplement providerInvoiceBuilderSupplement, VendorBill vendorBill) =>
        new(vendorBill.ExternalId,
            jobBillingDecorated.ContractType,
            jobBillingDecorated.JobBillingId,
            jobBillingDecorated.ProviderOrgId,
            jobBillingDecorated.JobWorkId,
            jobBillingDecorated.TicketId,
            jobBillingDecorated.CustomerId,
            jobBillingDecorated.PropertyId,
            jobBillingDecorated.ServiceLineId,
            providerInvoiceBuilderSupplement.ServiceLineName,
            jobBillingDecorated.ServiceTypeId,
            jobBillingDecorated.JobSummary.IfNone(NonEmptyText.NewUnsafe("Not Entered")),   // a missing job summary at this point should never occur
            jobBillingDecorated.TicketNumber,
            jobBillingDecorated.JobWorkNumber,
            providerInvoiceBuilderSupplement.ServicingAddressStateName.IfNone(NonEmptyText.NewUnsafe("Unknown")),    // Per Finance, sending "Unknown" to FAL is acceptable for short term data issue 
            TransactionDate:jobBillingDecorated.ProviderSubmittedOnDate.IfNone(DateTimeOffset.Now), // ProviderSubmittedOnDate is "optional" only because Fulfillment was not always setting it; should always have value going forward
            jobBillingDecorated.CostingScheme,
            ProviderInvoiceStatus:vendorBill.ProviderInvoiceStatus,
            DmgInvoiceNumber:providerInvoiceBuilderSupplement.DmgInvoiceNumber,
            TotalCost:jobBillingDecorated.TotalCost,
            // required sections
            ToProviderInvoiceVendorBill(vendorBill),
            // optional scalars
            ProviderInvoiceNumber:jobBillingDecorated.ProviderInvoiceNumber,
            // collections
            jobBillingDecorated.MaterialPart.LineItems.Map(jobBillingMaterialPartLineItem => ToProviderInvoiceMaterialPartLineItem(jobBillingMaterialPartLineItem.Core, providerInvoiceBuilderSupplement.CatalogItems)),
            jobBillingDecorated.Equipment.LineItems.Map(jobBillingEquipmentLineItem => ToProviderInvoiceEquipmentLineItem(jobBillingEquipmentLineItem.Core, providerInvoiceBuilderSupplement.CatalogItems)),
            jobBillingDecorated.Labor.LineItems.Map(ToProviderInvoiceLaborLineItem),
            jobBillingDecorated.TripCharge.LineItems.Where(x => !x.IsRequestedByDateMissed).Freeze().Map(ToProviderInvoiceTripChargeLineItem),
            jobBillingDecorated.JobFlatRate.LineItems.Map(jobBillingFlatRateLineItem => ToProviderInvoiceJobFlatRateLineItem(jobBillingFlatRateLineItem, providerInvoiceBuilderSupplement)),
            jobBillingDecorated.ProcessingFee.LineItems.Map(ToProviderInvoiceProcessingFeeLineItem),
            jobBillingDecorated.MaterialPart.CostDiscounts.Map(x => ToProviderInvoiceCostDiscountLineItem(x, FalItemType.MaterialPart)),
            jobBillingDecorated.Equipment.CostDiscounts.Map(x => ToProviderInvoiceCostDiscountLineItem(x, FalItemType.Equipment)),
            jobBillingDecorated.Labor.CostDiscounts.Map(x => ToProviderInvoiceCostDiscountLineItem(x, FalItemType.Labor)),
            ToProviderInvoiceCostDiscountLineItems(jobBillingDecorated.JobFlatRate));
    
    public static BillingJobAttachmentMeta ToBillingJobAttachmentMeta(PhotoFolder photoFolder) =>
        new BillingJobAttachmentMeta(
            photoFolder.BeforePhotosCount, 
            photoFolder.AfterPhotosCount);

    public static ProviderBillingJob ToProviderBillingJob(JobBillingJob jobBillingJob) =>
        new ProviderBillingJob(
            jobBillingJob.JobWorkId,
            jobBillingJob.ServiceTypeId,
            jobBillingJob.JobWorkNumber,
            jobBillingJob.Urgency,
            JobCompleteDate: jobBillingJob.JobCompleteDate,
            jobBillingJob.Scope,
            // optional scalars
            jobBillingJob.JobWorkState,
            jobBillingJob.JobWorkStatus,
            // required sections
            AttachmentMeta:ToBillingJobAttachmentMeta(jobBillingJob.PhotoFolder),
            // optional sections
            jobBillingJob.JobCondition,
            // optional collections
            jobBillingJob.Costings);
    
    public static ProviderBillingId ToProviderBillingId(JobBillingId jobBillingId) =>
        new ProviderBillingId(jobBillingId.Value);
    
    public static ProviderBillingJobGroup ToProviderBillingJobGroup(JobBillingJobGroup jobBillingJobGroup) =>
        new ProviderBillingJobGroup(jobBillingJobGroup.Jobs.Map(ToProviderBillingJob));
    
    public static ProviderBillingAssignee ToProviderBillingAssignee(JobBillingAssignee jobBillingAssignee) =>
        jobBillingAssignee switch 
        {
            JobBillingAssignee.Provider => ProviderBillingAssignee.Provider,
            JobBillingAssignee.Operations => ProviderBillingAssignee.Operations,
            _ => ProviderBillingRule.GetAssigneeDefault() 
        };

    public static ProviderBillingStatus ToProviderBillingStatus(JobBillingStatus jobBillingStatus) =>
        jobBillingStatus switch 
        {
            JobBillingStatus.Unspecified => ProviderBillingStatus.Unspecified,
            JobBillingStatus.Todo => ProviderBillingStatus.Todo,
            JobBillingStatus.InProgress => ProviderBillingStatus.InProgress,
            JobBillingStatus.Verified => ProviderBillingStatus.Verified,
            JobBillingStatus.Canceled => ProviderBillingStatus.Cancelled,
            _ => ProviderBillingStatus.Unspecified 
        };

    public static ProviderBillingCostingScheme ToProviderBillingCostingScheme(JobBillingCostingScheme jobBillingCostingScheme) =>
        jobBillingCostingScheme switch 
        {
            JobBillingCostingScheme.TimeAndMaterial => ProviderBillingCostingScheme.TimeAndMaterial,
            JobBillingCostingScheme.FlatRate => ProviderBillingCostingScheme.FlatRate,
            JobBillingCostingScheme.ServiceBased => ProviderBillingCostingScheme.ServiceBased,
            _ => ProviderBillingRule.GetNonRoutineProviderBillingCostingSchemeDefault()
        };
    
    public static ProviderBilling ToProviderBilling(JobBillingDecorated jobBillingDecorated) =>
        new ProviderBilling(
            ToProviderBillingId(jobBillingDecorated.JobBillingId),
            jobBillingDecorated.Version,
            false,
            jobBillingDecorated.ContractType,
            jobBillingDecorated.TicketId,
            TicketNumber: jobBillingDecorated.TicketNumber,
            jobBillingDecorated.ProviderOrgId,
            jobBillingDecorated.CustomerId,
            jobBillingDecorated.PropertyId,
            jobBillingDecorated.ServiceLineId,
            ToProviderBillingCostingScheme(jobBillingDecorated.CostingScheme),
            ToProviderBillingStatus(jobBillingDecorated.JobBillingStatus),
            TotalCost: jobBillingDecorated.TotalCost,
            ToProviderBillingAssignee(jobBillingDecorated.Assignee),
            SourceSystem: SourceSystem.Fulfillment,
            BillingType.NewNonRoutine(NonRoutineBillingType.Standard),
            // optional scalars
            Option<JobBillingId>.Some(jobBillingDecorated.JobBillingId),
            ProviderBillingNumber: Option<NonEmptyText>.None,   // TODO set from job "billing number"?
            JobSummary: jobBillingDecorated.JobSummary,
            Detail: Option<NonEmptyText>.None,  // Detail is routine only, not on job billing
            Note: Option<NonEmptyText>.None,    // Note is routine only, not on job billing
            ProviderInvoiceNumber: jobBillingDecorated.ProviderInvoiceNumber,
            ProviderFirstSubmittedOnDate: jobBillingDecorated.ProviderFirstSubmittedOnDate,
            ProviderLastSubmittedOnDate: jobBillingDecorated.ProviderLastSubmittedOnDate,
            Option<DateTimeOffset>.None,
            PsaId:Option<PsaId>.None,
            // required sections
            jobBillingDecorated.RecordMeta,
            jobBillingDecorated.MaterialPart,
            jobBillingDecorated.Equipment,
            jobBillingDecorated.Labor,
            jobBillingDecorated.TripCharge,
            jobBillingDecorated.JobFlatRate,
            jobBillingDecorated.MaterialPartFlatRate,
            jobBillingDecorated.EquipmentFlatRate,
            new ProviderBillingVisitDetail(new Lst<ProviderBillingVisit>()),
            new ProviderBillingBillingDetail(new Lst<ProviderBillingBillingLineItem>()),
            jobBillingDecorated.ProcessingFee,
            jobBillingDecorated.Payment,
            ToProviderBillingJobGroup(jobBillingDecorated.Job),
            new ProviderBillingDiscount(0,0, Option<Guid>.None),            //TODO spoke with the front end, it ok to make this all 0 for non routine,
            // eventually we want probably want to sum up the discounts for each line item so it matches
            // optional sections
            jobBillingDecorated.SubmissionDetailLatest,
            jobBillingDecorated.Additional,
            Option<Event>.None,
            Option<ProviderBillingTripChargeLineItem>.None,
            Option<WeatherWorks>.None,
            // required collection
            jobBillingDecorated.RuleMessages,
            Lst<MultiVisitJobRate>.Empty);
}