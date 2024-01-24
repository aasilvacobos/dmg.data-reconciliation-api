using DMG.ProviderInvoicing.BL.Utility;
using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain.Rule;

/// Rules and calculations related to the job billing gross
public static class JobBillingGrossRule
{
    /// Is a job billing using flat rate costing
    public static bool IsFlatRateCosted(JobBillingGross jobBillingGross) =>
        jobBillingGross.CostingScheme == JobBillingCostingScheme.FlatRate  // Unspecified costing scheme will be considered T&M
        || jobBillingGross.JobFlatRate.LineItems.Count > 0;      // Flat rate costing can still have line items for material/part, equipment, etc.
    
    #region Trip Charge
    /// <summary>
    /// Function to determine if the line item has a single message with a billing rule of <see cref="JobBillingMessageRule.AddMissedArrivalLineItemBasedOnUrgencyTrip"/>.
    /// </summary>
    /// <param name="lineItem">The line item to evaluate.</param>
    /// <returns><b>true</b> if the line item contains a single message with the billing rule, <b>false</b> if not.</returns>
    /// TODO this will be eliminated after logic is moved into new job billing SOR
    public static bool IsAddMissedArrivalLineItemBasedOnUrgencyTrip(JobBillingGrossTripChargeLineItem lineItem) =>
        lineItem.RuleMessages.Any(x => x.MessageRule == JobBillingMessageRule.AddMissedArrivalLineItemBasedOnUrgencyTrip);
    #endregion
}
