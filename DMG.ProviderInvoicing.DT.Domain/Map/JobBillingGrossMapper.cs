using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Domain.Rule;
using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain.Map; 

/// Functions to map the job billing gross to other entities
public static class JobBillingGrossMapper 
{
    //////////////////////////////////////////////////////////////////////
    // Mappings to JobBillingDecorated entity

    private static JobBillingDecoratedMaterialPartLineItem ToJobBillingDecoratedMaterialPartLineItem(JobBillingGrossMaterialPartEquipmentLineItem lineItem) =>
        new (ToJobBillingMaterialPartLineItem(lineItem),
            lineItem.Dispute);
    
    private static JobBillingDecoratedMaterialPart ToJobBillingDecoratedMaterialPart(JobBillingGrossMaterialPart materialPart) =>
        new JobBillingDecoratedMaterialPart(
            materialPart.LineItems.Map(ToJobBillingDecoratedMaterialPartLineItem),
            materialPart.CostDiscounts.Map(ToJobBillingCostDiscountLineItem),
            materialPart.RuleMessages);

    private static JobBillingDecoratedEquipmentLineItem ToJobBillingDecoratedEquipmentLineItem(JobBillingGrossMaterialPartEquipmentLineItem lineItem) =>
        new (ToJobBillingEquipmentLineItem(lineItem),
            lineItem.Dispute);

    private static JobBillingDecoratedEquipment ToJobBillingDecoratedEquipment(JobBillingGrossEquipment equipment) =>
        new JobBillingDecoratedEquipment(
            equipment.LineItems.Map(ToJobBillingDecoratedEquipmentLineItem),
            equipment.CostDiscounts.Map(ToJobBillingCostDiscountLineItem),
            equipment.RuleMessages);
    
    private static JobBillingDecoratedLaborLineItemTechnicianTrip ToJobBillingDecoratedLaborLineItemTechnicianTrip(JobBillingGrossTechnicianTrip jobBillingGrossTechnicianTrip) =>
        new (jobBillingGrossTechnicianTrip.TechnicianTripId,
            jobBillingGrossTechnicianTrip.TechnicianUserId,
            jobBillingGrossTechnicianTrip.CheckInDateTime,
            jobBillingGrossTechnicianTrip.CheckOutDateTime,
            jobBillingGrossTechnicianTrip.TotalBillableTimeInSeconds,
            jobBillingGrossTechnicianTrip.IsPayable,
            jobBillingGrossTechnicianTrip.CreationSource);
    
    private static IJobBillingDecoratedLaborLineItem ToJobBillingDecoratedLaborLineItem(JobBillingGrossLaborLineItem laborLineItem) => 
        new JobBillingDecoratedLaborLineItem(
            laborLineItem.Id,
            laborLineItem.JobWorkId,
            laborLineItem.TechnicianType,
            laborLineItem.AdjustedTotalBillableTimeInHours,
            laborLineItem.TotalBillableTimeInSeconds,
            laborLineItem.AdjustedTotalBillableTimeInSeconds,
            laborLineItem.LaborRate,
            laborLineItem.LineItemCost,
            laborLineItem.RateType,
            laborLineItem.Dispute,
            laborLineItem.TimeAdjustments.Select(ToJobBillingLaborLineItemTimeAdjustment),
            laborLineItem.TechnicianTrips.Select(ToJobBillingDecoratedLaborLineItemTechnicianTrip),
            laborLineItem.RuleMessages);
    
    private static JobBillingDecoratedLabor ToJobBillingDecoratedLabor(JobBillingGrossLabor labor, Lst<Costing> costings) =>
        new JobBillingDecoratedLabor(
            labor.TotalCost,
            labor.LaborLineItems.Map(ToJobBillingDecoratedLaborLineItem),
            labor.CostDiscounts.Map(ToJobBillingCostDiscountLineItem),
            labor.RuleMessages);

    private static JobBillingJobFlatRate ToJobBillingJobFlatRate(JobBillingJobFlatRate flatRate) =>
        new JobBillingJobFlatRate(
            flatRate.AdjustedTotalCost,
            flatRate.LineItems.Map(ToJobBillingJobFlatRateLineItem),
            flatRate.CostDiscounts.Map(ToJobBillingCostDiscountLineItem),
            flatRate.RuleMessages);
    
    private static JobBillingDecoratedMaterialPartFlatRateLineItem ToJobBillingDecoratedMaterialPartFlatRateLineItem(JobBillingGrossMaterialPartEquipmentFlatRateLineItem lineItem) =>
        new (ToJobBillingMaterialPartFlatRateLineItem(lineItem),
            lineItem.Dispute);
    
    private static JobBillingDecoratedMaterialPartFlatRate ToJobBillingDecoratedMaterialPartFlatRate(JobBillingGrossMaterialPartFlatRate materialPart) =>
        new JobBillingDecoratedMaterialPartFlatRate(
            materialPart.AdjustedTotalCost,
            materialPart.LineItems.Map(ToJobBillingDecoratedMaterialPartFlatRateLineItem),
            materialPart.CostDiscounts.Map(ToJobBillingCostDiscountLineItem),
            materialPart.RuleMessages);
    
    private static JobBillingDecoratedEquipmentFlatRateLineItem ToJobBillingDecoratedEquipmentFlatRateLineItem(JobBillingGrossMaterialPartEquipmentFlatRateLineItem lineItem) =>
        new (ToJobBillingEquipmentFlatRateLineItem(lineItem),
            lineItem.Dispute);

    private static JobBillingDecoratedEquipmentFlatRate ToJobBillingDecoratedEquipmentFlatRate(JobBillingGrossEquipmentFlatRate equipment) =>
        new JobBillingDecoratedEquipmentFlatRate(
            equipment.AdjustedTotalCost,
            equipment.LineItems.Map(ToJobBillingDecoratedEquipmentFlatRateLineItem),
            equipment.CostDiscounts.Map(ToJobBillingCostDiscountLineItem),
            equipment.RuleMessages);

    private static Lst<JobBillingJob> ToJobBillingJobs(Job job, Lst<JobPhoto> jobPhotos) =>
        List(new 
            JobBillingJob(
                job.JobWorkId,
                job.ServiceTypeId,
                JobWorkNumber: NonEmptyText.NewUnsafe(job.JobWorkNumber),
                job.Urgency,
                job.JobCompleteDate,
                Scope: NonEmptyText.NewUnsafe(job.Scope),
                Option<JobWorkState>.Some(job.JobWorkState), 
                Option<JobWorkStatus>.Some(job.JobWorkStatus),
                PhotoMapper.ToPhotoFolder(jobPhotos),
                job.JobCondition,
                Option<Lst<Costing>>.Some(job.Costings)));

    private static JobBillingJobGroup ToJobBillingJobGroup(Job job, Lst<JobPhoto> jobPhotos) =>
        new JobBillingJobGroup(
            ToJobBillingJobs(job, jobPhotos));

    /// Map to the JobBillingDecorated from the JobBillingGross. 
    public static JobBillingDecorated ToJobBillingDecorated(JobBillingGross jobBillingGross, Job job) =>
        new (jobBillingGross.JobBillingId,
            jobBillingGross.JobWorkId,
            jobBillingGross.Version,
            JobBillingRule.GetJobBillingContractType(),
            job.TicketId,
            job.ProviderOrgId,
            job.CustomerId,
            job.PropertyId,
            job.ServiceLineId,
            job.ServiceTypeId,
            NonEmptyText.NewUnsafe(job.JobWorkNumber),
            NonEmptyText.NewUnsafe(job.TicketNumber),
            job.JobCompleteDate,
            jobBillingGross.CostingScheme,
            jobBillingGross.JobBillingStatus,
            JobBillingRule.CalculateTotalCost(jobBillingGross),
            jobBillingGross.Assignee,
            jobBillingGross.JobSummary,
            jobBillingGross.ProviderInvoiceNumber,
            jobBillingGross.ProviderSubmittedOnDate,
            jobBillingGross.ProviderFirstSubmittedOnDate,
            jobBillingGross.ProviderLastSubmittedOnDate,
            jobBillingGross.RecordMeta,
            ToJobBillingDecoratedMaterialPart(jobBillingGross.MaterialPart),
            ToJobBillingDecoratedEquipment(jobBillingGross.Equipment),
            ToJobBillingDecoratedLabor(jobBillingGross.Labor, job.Costings),
            ToJobBillingTripCharge(jobBillingGross.TripChargeLineItems),
            ToJobBillingJobFlatRate(jobBillingGross.JobFlatRate),
            ToJobBillingDecoratedMaterialPartFlatRate(jobBillingGross.MaterialPartFlatRate),
            ToJobBillingDecoratedEquipmentFlatRate(jobBillingGross.EquipmentFlatRate),
            JobBillingRule.BuildProcessingFeeSection(jobBillingGross), 
            jobBillingGross.Payment,
            Job:ToJobBillingJobGroup(job, jobBillingGross.Photos),
            PhotoMapper.ToPhotoFolder(jobBillingGross.Photos),
            jobBillingGross.SubmissionDetailLatest,
            jobBillingGross.Additional,
            jobBillingGross.RuleMessages);
    
    //////////////////////////////////////////////////////////////////////////////////////////////
    /// mappings to JobBilling entity
    
    public static IJobBillingMaterialPartEquipmentItem ToJobBillingMaterialPartEquipmentItem(IJobBillingGrossMaterialPartEquipmentItem iJobBillingGrossMaterialPartEquipmentItem) =>
        iJobBillingGrossMaterialPartEquipmentItem switch 
        {
            JobBillingGrossMaterialPartEquipmentCatalogItem item =>
                new JobBillingMaterialPartEquipmentCatalogItem(item.CatalogItemId, item.RuleValues) as IJobBillingMaterialPartEquipmentItem, 
            JobBillingGrossMaterialPartEquipmentNonCatalogItem item => 
                new JobBillingMaterialPartEquipmentNonCatalogItem(NonEmptyText.NewUnsafe(item.Name)) as IJobBillingMaterialPartEquipmentItem,
            _ => 
                // valid case to throw because if a new class implements interface, we need to know immediately know that it is not implemented here
                throw new ArgumentException($"Invalid IJobBillingGrossMaterialPartEquipmentItem item argument {nameof(iJobBillingGrossMaterialPartEquipmentItem)}.") 
        };
    
    private static IJobBillingCostDiscountLineItem ToJobBillingCostDiscountLineItem(JobBillingGrossCostDiscountLineItem lineItem) => // TODO change "Adjustment" to "Discount"
        new JobBillingCostDiscountLineItem(
            lineItem.CostDiscount,
            lineItem.CreationSource,
            lineItem.RecordMeta);

    private static IJobBillingCostDiscountLineItem ToJobBillingCostDiscountLineItem(IJobBillingCostDiscountLineItem lineItem) => 
        new JobBillingCostDiscountLineItem(
            lineItem.CostDiscount,
            lineItem.CreationSource,
            lineItem.RecordMeta);
    
    private static JobBillingMaterialPartLineItem ToJobBillingMaterialPartLineItem(JobBillingGrossMaterialPartEquipmentLineItem lineItem) =>
        new JobBillingMaterialPartLineItem(
            lineItem.Id,
            CatalogItemTypeMapper.ToMaterialPartCatalogItemType(lineItem.CatalogItemType),
            ToJobBillingMaterialPartEquipmentItem(lineItem.Item),
            lineItem.Quantity,
            lineItem.UnitType,
            new(lineItem.ItemCost),
            lineItem.LineItemCost,
            lineItem.CreationSource,
            lineItem.JobWorkId,
            lineItem.Reason,
            lineItem.RecordMeta,
            lineItem.RuleMessages);
    
    private static JobBillingMaterialPart ToJobBillingMaterialPart(JobBillingGrossMaterialPart materialPart) => // TODO move to this method
        new JobBillingMaterialPart(
            materialPart.LineItems.Map(ToJobBillingDecoratedMaterialPartLineItem),
            materialPart.CostDiscounts.Map(ToJobBillingCostDiscountLineItem),
            materialPart.RuleMessages);
    
    private static JobBillingEquipmentLineItem ToJobBillingEquipmentLineItem(JobBillingGrossMaterialPartEquipmentLineItem lineItem) =>
        new JobBillingEquipmentLineItem(
            lineItem.Id,
            CatalogItemTypeMapper.ToEquipmentCatalogItemType(lineItem.CatalogItemType),
            ToJobBillingMaterialPartEquipmentItem(lineItem.Item),
            lineItem.Quantity,
            lineItem.UnitType,
            new(lineItem.ItemCost),
            lineItem.LineItemCost,
            lineItem.CreationSource,
            lineItem.JobWorkId,
            lineItem.Reason,
            lineItem.RecordMeta,
            lineItem.RuleMessages);
    
    private static JobBillingEquipment ToJobBillingEquipment(JobBillingGrossEquipment equipment) => // TODO move to this method
        new JobBillingEquipment(
            equipment.LineItems.Map(ToJobBillingDecoratedEquipmentLineItem),
            equipment.CostDiscounts.Map(ToJobBillingCostDiscountLineItem),
            equipment.RuleMessages);
   
    private static IJobBillingLaborLineItemTimeAdjustment ToJobBillingLaborLineItemTimeAdjustment(JobBillingGrossLaborLineItemTimeAdjustment jobBillingGrossLaborLineItemTimeAdjustment) =>
        new JobBillingLaborLineItemTimeAdjustment(
            TimeConverter.ToAdjustmentMinutes(jobBillingGrossLaborLineItemTimeAdjustment.TimeAdjustmentInSeconds),
            jobBillingGrossLaborLineItemTimeAdjustment.Cost,
            jobBillingGrossLaborLineItemTimeAdjustment.IsProviderConfirmed,
            jobBillingGrossLaborLineItemTimeAdjustment.CreationSource,
            jobBillingGrossLaborLineItemTimeAdjustment.Reason,
            jobBillingGrossLaborLineItemTimeAdjustment.RecordMeta,
            jobBillingGrossLaborLineItemTimeAdjustment.RuleMessages);

    private static IJobBillingLaborLineItem ToJobBillingLaborLineItem(JobBillingGrossLaborLineItem laborLineItem) =>
        new JobBillingLaborLineItem(
            laborLineItem.Id,
            laborLineItem.JobWorkId,
            laborLineItem.TechnicianType,
            laborLineItem.AdjustedTotalBillableTimeInHours,
            laborLineItem.LaborRate,
            laborLineItem.LineItemCost,
            laborLineItem.RateType,
            laborLineItem.TimeAdjustments.Select(ToJobBillingLaborLineItemTimeAdjustment),
            laborLineItem.RuleMessages);
    
    /// Map trip charge line item from job billing gross to job billing
    private static JobBillingTripChargeLineItem ToJobBillingTripChargeLineItem(int index, JobBillingGrossTripChargeLineItem lineItem) =>
        new (JobBillingRule.BuildTripChargeLineItemDescription(index), 
            lineItem.RequestedByDateTime,
            lineItem.FirstTechnicianArrivalDateTime, 
            lineItem.RateType,
            lineItem.TripChargeRate,
            lineItem.LineItemCost,
            lineItem.IsTripPayable,
            JobBillingGrossRule.IsAddMissedArrivalLineItemBasedOnUrgencyTrip(lineItem),
            lineItem.CreationSource);

    public static JobBillingTripCharge ToJobBillingTripCharge(Lst<JobBillingGrossTripChargeLineItem> tripChargeLineItems) =>
        new JobBillingTripCharge(
            tripChargeLineItems
                // do not include a trip charge line item unless it is payable TODO rethink this because we are removing a line item that Fulfillment does not (during a mapping!)
                .Filter(x => x.IsTripPayable || JobBillingGrossRule.IsAddMissedArrivalLineItemBasedOnUrgencyTrip(x))
                .OrderBy(x => x.RequestedByDateTime)
                .Map(ToJobBillingTripChargeLineItem)
                .Freeze());

    private static JobBillingJobFlatRateLineItem ToJobBillingJobFlatRateLineItem(JobBillingJobFlatRateLineItem lineItem) =>
        new JobBillingJobFlatRateLineItem(
            lineItem.Id,
            lineItem.Quantity,
            lineItem.Rate,
            lineItem.LineItemCost,
            lineItem.IsItemPayable,
            lineItem.IsFlaggedBySystem,
            lineItem.IsManuallyVerified,
            lineItem.CreationSource,
            lineItem.Reason,
            lineItem.RuleMessages);
    
    private static JobBillingMaterialPartFlatRateLineItem ToJobBillingMaterialPartFlatRateLineItem(JobBillingGrossMaterialPartEquipmentFlatRateLineItem lineItem) =>
        new JobBillingMaterialPartFlatRateLineItem(
            lineItem.Id,
            lineItem.Quantity,
            lineItem.Rate,
            lineItem.LineItemCost,
            lineItem.CatalogItemId,
            CatalogItemTypeMapper.ToMaterialPartCatalogItemType(lineItem.CatalogItemType),
            IsItemPayable:lineItem.IsItemPayable,
            IsFlaggedBySystem:lineItem.IsFlaggedBySystem,
            IsManuallyVerified:lineItem.IsManuallyVerified,
            lineItem.CreationSource,
            lineItem.Reason,
            lineItem.RecordMeta,
            lineItem.RuleMessages);

    private static JobBillingEquipmentFlatRateLineItem ToJobBillingEquipmentFlatRateLineItem(JobBillingGrossMaterialPartEquipmentFlatRateLineItem lineItem) =>
        new JobBillingEquipmentFlatRateLineItem(
            lineItem.Id,
            lineItem.Quantity,
            lineItem.Rate,
            lineItem.LineItemCost,
            lineItem.CatalogItemId,
            CatalogItemTypeMapper.ToEquipmentCatalogItemType(lineItem.CatalogItemType),
            IsItemPayable:lineItem.IsItemPayable,
            IsFlaggedBySystem:lineItem.IsFlaggedBySystem,
            IsManuallyVerified:lineItem.IsManuallyVerified,
            lineItem.CreationSource,
            lineItem.Reason,
            lineItem.RecordMeta,
            lineItem.RuleMessages);
    
    /// Construct a job billing from the job and job billing gross
    public static JobBilling ToJobBilling(DT.Domain.Job job, DT.Domain.JobBillingGross jobBillingGross) =>
        new (jobBillingGross.JobBillingId,
            jobBillingGross.JobWorkId,
            jobBillingGross.Version,
            JobBillingRule.GetJobBillingContractType(),
            job.TicketId,
            job.ProviderOrgId,
            job.CustomerId,
            job.PropertyId,
            job.ServiceLineId,
            job.ServiceTypeId,
            NonEmptyText.NewUnsafe(job.TicketNumber),
            NonEmptyText.NewUnsafe(job.JobWorkNumber),
            job.JobCompleteDate,
            jobBillingGross.CostingScheme,
            jobBillingGross.JobBillingStatus,
            jobBillingGross.TotalCost,
            jobBillingGross.Assignee,
            jobBillingGross.JobSummary,
            jobBillingGross.ProviderInvoiceNumber,
            jobBillingGross.ProviderSubmittedOnDate,
            jobBillingGross.ProviderFirstSubmittedOnDate,
            jobBillingGross.ProviderLastSubmittedOnDate,
            jobBillingGross.RecordMeta,
            new JobBillingMaterialPart(
                jobBillingGross.MaterialPart.LineItems.Select(ToJobBillingDecoratedMaterialPartLineItem),
                jobBillingGross.MaterialPart.CostDiscounts.Map(ToJobBillingCostDiscountLineItem),
                jobBillingGross.MaterialPart.RuleMessages),
            new JobBillingEquipment(
                jobBillingGross.Equipment.LineItems.Select(ToJobBillingDecoratedEquipmentLineItem),
                jobBillingGross.Equipment.CostDiscounts.Map(ToJobBillingCostDiscountLineItem),
                jobBillingGross.Equipment.RuleMessages),
            new JobBillingLabor(
                jobBillingGross.Labor.TotalCost,
                jobBillingGross.Labor.LaborLineItems.Map(ToJobBillingDecoratedLaborLineItem),
                jobBillingGross.Labor.CostDiscounts.Map(ToJobBillingCostDiscountLineItem),
                jobBillingGross.Labor.RuleMessages),
            ToJobBillingTripCharge(jobBillingGross.TripChargeLineItems),
            new JobBillingJobFlatRate(
                jobBillingGross.JobFlatRate.AdjustedTotalCost,
                jobBillingGross.JobFlatRate.LineItems.Map(ToJobBillingJobFlatRateLineItem),
                jobBillingGross.JobFlatRate.CostDiscounts.Map(ToJobBillingCostDiscountLineItem),
                jobBillingGross.JobFlatRate.RuleMessages),
            new JobBillingDecoratedMaterialPartFlatRate(
                jobBillingGross.MaterialPartFlatRate.AdjustedTotalCost,
                jobBillingGross.MaterialPartFlatRate.LineItems.Map(ToJobBillingDecoratedMaterialPartFlatRateLineItem),
                jobBillingGross.MaterialPartFlatRate.CostDiscounts.Map(ToJobBillingCostDiscountLineItem),
                jobBillingGross.MaterialPartFlatRate.RuleMessages),
            new JobBillingDecoratedEquipmentFlatRate(
                jobBillingGross.EquipmentFlatRate.AdjustedTotalCost,
                jobBillingGross.EquipmentFlatRate.LineItems.Map(ToJobBillingDecoratedEquipmentFlatRateLineItem),
                jobBillingGross.EquipmentFlatRate.CostDiscounts.Map(ToJobBillingCostDiscountLineItem),
                jobBillingGross.EquipmentFlatRate.RuleMessages),
            JobBillingRule.BuildProcessingFeeSection(jobBillingGross),
            jobBillingGross.Payment,
            Job:ToJobBillingJobGroup(job, jobBillingGross.Photos),
            PhotoMapper.ToPhotoFolder(jobBillingGross.Photos),
            jobBillingGross.SubmissionDetailLatest,
            jobBillingGross.Additional,
            jobBillingGross.RuleMessages);
}