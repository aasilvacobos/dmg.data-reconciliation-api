namespace DMG.ProviderInvoicing.DT.Domain

type BillingType =
    | NonRoutine of NonRoutineBillingType
    | Routine of RoutineBillingType
    | Undefined 
and NonRoutineBillingType =
    /// NONROUTINE_STANDARD
    | Standard // 
    /// NONROUTINE_SNOW
    | Snow // Time and Material
    /// NONROUTINE_LOT_AND_LAND
    | LotAndLand //    
    | Undefined
and RoutineBillingType =
    /// ROUTINE_PER_OCCURRENCE
    | PerOccurrence
    /// ROUTINE_PER_EVENT
    | PerEvent
    /// ROUTINE_SEASONAL_FLAT_RATE
    | SeasonalFlatRate 
    /// ROUTINE_SEASONAL_VARIABLE
    | SeasonalVariable
    /// ROUTINE_SEASONAL_TIERED
    | SeasonalTiered
    /// ROUTINE_SEASONAL_HYBRID
    | SeasonalHybrid
    /// ROUTINE_TIME_AND_MATERIAL
    | TimeAndMaterial     
    | Undefined
