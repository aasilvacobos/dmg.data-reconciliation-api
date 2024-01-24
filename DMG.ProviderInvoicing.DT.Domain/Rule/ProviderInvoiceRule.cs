namespace DMG.ProviderInvoicing.DT.Domain.Rule;

public static class ProviderInvoiceRule
{
    /// Is a provider invoice flat rate costed.
    public static bool IsFlatRateCosted(ProviderInvoiceInsert providerInvoice) =>
        // TODO eliminate line items condition, CostingScheme should suffice
        providerInvoice.CostingScheme == JobBillingCostingScheme.FlatRate     // Unspecified costing scheme will be considered T&M
        || providerInvoice.JobFlatRateLineItems.Count > 0;          // Flat rate costing can still have line items for material/part, equipment, etc.
}