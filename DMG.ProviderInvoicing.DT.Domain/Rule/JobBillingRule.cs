using DMG.ProviderInvoicing.BL.Utility;
using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain.Rule; 

/// Rules and calculations related to the job billing
public static class JobBillingRule
{
    /// Is there either a) at least 1 credit card payment, or b) a credit card payment terms?
    public static bool IsPaidByCreditCard(JobBillingPayment jobBillingPaymentSection) =>
        jobBillingPaymentSection.PaymentTerms.Map(x => x.Value.Trim().ToLower()) == Option<string>.Some("paid by credit card")
        || jobBillingPaymentSection.Payments.Exists(x => x.PaymentMethod is JobBillingPaymentMethodCreditCard);
    
    /// Is a job billing using flat rate costing
    public static bool IsFlatRateCosted(JobBilling jobBilling) =>
        jobBilling.CostingScheme == JobBillingCostingScheme.FlatRate  // Unspecified costing scheme will be considered T&M
        || jobBilling.JobFlatRate.LineItems.Count > 0;         // Flat rate costing can still have line items for material/part, equipment, etc.
    
    // TODO replace hardcoded processing fee with processing fee data received from Fulfillment message
    // "I think that it should come from JobBilling and be applied by the job billing
    // rules engine - but until that time here it is." -Neil
    // TODO "Deliver us from this cursed hard coding. Amen."
    public static JobBillingProcessingFee BuildProcessingFeeSection(JobBillingGross jobBillingGross) =>
        jobBillingGross.Payment.IsPaidByCreditCard || jobBillingGross.TotalCost < 1.0M
            // Business rule: Exclude $1.00 processing fee if there is a credit card payment or total amount is less than $1.00
            ? new (Lst<JobBillingProcessingFeeLineItem>.Empty) 
            : new (List(new JobBillingProcessingFeeLineItem(NonEmptyText.NewUnsafe(@"Processing Fee"), new ProcessingFee(-1.00M))));

    /// Calculate the total cost for a job billing from the job billing gross
    public static decimal CalculateTotalCost(JobBillingGross jobBillingGross) =>
        jobBillingGross.TotalCost
        // add in processing fee since it is not part of the job billing data received from Fulfillment
        + BuildProcessingFeeSection(jobBillingGross).LineItems
            .Map(x => x.ProcessingFee.Value)
            .Sum();
    
    /// Determine all unique catalog item ids from a materialPart and equipment section of a job billing
    /// TODO Move to version that accepts entire job billing
    public static Lst<CatalogItemId> GetItemsUniqueCatalogItemIds(JobBillingMaterialPart jobBillingMaterialPart, JobBillingEquipment jobBillingEquipment) =>
        // combine material/part and equipment items into one collection
        jobBillingMaterialPart.LineItems.Map(li => li.Core.Item)
            .Append(jobBillingEquipment.LineItems.Map(li => li.Core.Item))
            .Map(x => x
                switch {
                    JobBillingMaterialPartEquipmentCatalogItem item => Some(item.CatalogItemId),
                    JobBillingMaterialPartEquipmentNonCatalogItem => None, // don't care about non-catalog
                    _ => None
                })
            .Bind(x => x.ToList())
            .Distinct()
            .Freeze();

    /// Determine all unique catalog item ids from a materialPart and equipment section of a job billing
    public static Lst<CatalogItemId> GetItemsUniqueCatalogItemIds(JobBillingDecorated jobBillingDecorated) =>
        // combine material/part and equipment items into one collection
        jobBillingDecorated.MaterialPart.LineItems.Map(li => li.Core.Item)
            .Append(jobBillingDecorated.Equipment.LineItems.Map(li => li.Core.Item))
            .Map(x => x
                switch {
                    JobBillingMaterialPartEquipmentCatalogItem item => Some(item.CatalogItemId),
                    JobBillingMaterialPartEquipmentNonCatalogItem => None, // don't care about non-catalog
                    _ => None
                })
            .Bind(x => x.ToList())
            .Distinct()
            .Freeze();
    
    // Is a rule message visible to the provider?
    public static bool IsRuleMessageVisible(JobBillingRuleMessage ruleMessage) =>
        ruleMessage.Visibilities.Contains(JobBillingMessageVisibility.Provider);

    public static DT.Domain.TechnicianType GetTechnicianTypeDefault() =>
        DT.Domain.TechnicianType.TechnicianTypeRegular;
    
    public static DT.Domain.JobBillingAssignee GetAssigneeDefault() =>
        DT.Domain.JobBillingAssignee.Operations;

    public static BillingContractType GetJobBillingContractType() =>
        BillingContractType.NonRoutine;   // Job billing is always Non-Routine. Routine is only possible on provider billing.
        
    #region TripCharge
    public static decimal CalculateTripChargeTotalCost(Lst<JobBillingTripChargeLineItem> tripChargeLineItems) =>
        tripChargeLineItems
            .Filter(x => !x.IsRequestedByDateMissed)
            .Map(x => x.LineItemCost.Value)
            .Sum();

    /// <summary>
    /// Builds the description for a trip charge line item based on its index
    /// </summary>
    /// <param name="lineItemIndex">0-based index of the line item</param>
    /// <returns></returns>
    public static NonEmptyText BuildTripChargeLineItemDescription(int lineItemIndex) =>
        NonEmptyText.NewUnsafe($"Trip charge #{lineItemIndex + 1}");
    #endregion
}