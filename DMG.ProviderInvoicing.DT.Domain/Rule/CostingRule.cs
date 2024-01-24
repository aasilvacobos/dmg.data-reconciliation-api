using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain.Rule;

public static class CostingRule 
{
    /// Choose costing to be used from costing collection
    public static Option<Costing> TryChooseCosting(Lst<Costing> costings) =>
        costings
            // We expect multiple rate types in future. For now, we only implement for Regular.
            .Filter(costing => costing.RateType == RateType.Regular)
            .ToOption();
    
    public static DT.Domain.RateType GetRateTypeDefault() =>
        DT.Domain.RateType.Regular;
}