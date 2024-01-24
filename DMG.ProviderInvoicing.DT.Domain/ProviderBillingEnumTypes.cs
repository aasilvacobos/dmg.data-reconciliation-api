namespace DMG.ProviderInvoicing.DT.Domain;

/// Current assignee of a provider billing
public enum ProviderBillingAssignee 
{
    DistrictManager,
    Operations,
    Provider
}

/// Contract type of a job/provider billing. A job billing has an implicit contract type of NonRoutine.
public enum BillingContractType 
{
    NonRoutine,
    Routine,
    Undefined
}

public enum SourceSystem
{
    Fulfillment,
    ProviderBilling
}

public enum ModifyBy
{
    DistrictManager,
    Provider
}

public enum ModifyAction
{
    Add,
    Edit
}

public enum EventGroupSource
{
    Event,
    Manual_Grouping
}

public enum ProviderBillingSource
{
    DMG,
    PSA,
    Provider,
    VisitLogs,
    System
}

public enum ServiceItemSource
{
    Unspecified,
    LookupItem,
    ProviderAgreement,
    CostingEngine
}
