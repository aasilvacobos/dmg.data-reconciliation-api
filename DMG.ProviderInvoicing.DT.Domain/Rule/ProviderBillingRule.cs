using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain.Rule;

public static class ProviderBillingRule
{
    public static DT.Domain.ProviderBillingAssignee GetAssigneeDefault() =>
        // Safest to use Operations instead of DistrictManager because it is backward compatible with Non-Routine
        DT.Domain.ProviderBillingAssignee.Operations;
    
    public static ProviderBillingCostingScheme GetNonRoutineProviderBillingCostingSchemeDefault() =>
        ProviderBillingCostingScheme.TimeAndMaterial;
}