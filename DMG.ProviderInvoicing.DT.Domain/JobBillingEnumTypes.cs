using System;

namespace DMG.ProviderInvoicing.DT.Domain;

/// Status of a job billing's review/verification
public enum JobBillingStatus 
{
    Unspecified,
    Todo,
    InProgress,
    Verified,
    Canceled
}

/// Current assignee of a job billing
public enum JobBillingAssignee 
{
    Operations,
    Provider
}

// Types of billing rules.
public enum JobBillingMessageRule
{
    Unspecified,
    RemoveMinCheckInCheckOutLabor,
    RoundUpToNextMinLabor,
    QtyOfLaborForThisServiceType,
    LaborTotalCalculation,
    RemoveExtraTripsForSameDaySameProperty,
    RemoveExtraTrips,
    AddMissedArrivalLineItemBasedOnUrgencyTrip,
    TripTotalCalculation,
    RemoveUnPayablePartsAndMaterial,
    CheckVarianceOfQtyUsageForServiceTypePartsAndMaterial,
    CheckForNonCataloguePartsAndMaterial,
    CheckVarianceForRateComparedToOurStandardsPerServiceTypeForPartsAndMaterial,
    PartsAndMaterialTotalCalculation,
    RemoveUnPayableEquipments,
    CheckVarianceOfQtyUsageForServiceTypeEquipment,
    CheckForNonCatalogueEquipment,
    CheckVarianceForRateComparedToOurStandardsPerServiceTypeEquipment,
    EquipmentTotalCalculation,
    TotalAmountCalculation,
    CheckTotalAmountAgainstNte,
    CheckAverageTotalAmountAgainstPreviousWorkOfSameServiceLine,
    CheckPartsAndMaterialCatalogItemRate,
    CheckEquipmentCatalogItemRate,
    FlatRateJobTotalCalculation,
    CheckFlatRateJobTotalAmountIsNegative,
    FlatRatePartsAndMaterialTotalCalculation,
    CheckFlatRatePartsAndMaterialTotalAmountIsNegative,
    FlatRateEquipmentTotalCalculation,
    CheckFlatRateEquipmentTotalAmountIsNegative,
    MaxUsageHoursCalculation,
    CheckIsCreditCardProvider,
    // Deprecated cases
    //FlagNonCatalogItemsDescriptionPartsAndMaterial,
    //FlagNonCatalogItemsDescriptionEquipment,
    FlagNonCatalogItemsNamePartsAndMaterial,
    FlagNonCatalogItemsNameEquipment,
    AutoDeductLabor
}

// The level of visibility for a job billing message.
public enum JobBillingMessageVisibility 
{
    Operations,
    Provider
}

// The type of message.
public enum JobBillingMessageType
{
    Warning,
    Note,
    Alert
}

/// Chronology category of a photo        
public enum PhotoChronology 
{
    BeforePhoto,
    DuringPhoto,
    AfterPhoto, 
    OtherPhoto
}

/// Source that created an element in a collection on a job billing entity. Ex: equipment line item, tech trip, etc. 
public enum JobBillingElementCreationSource
{
    Unspecified,
    System,
    Provider,
    Technician,
    Operations,
    JobCosting
}

/// Reason for a job billing line item dispute request
public enum JobBillingLineItemDisputeRequestReason
{
    WhyWereSoManyHoursUsedToDoTheJob,
    PleaseShareTheReasonWhyThisMuchQuantityWasUsed,
    TheRateIsHighCanYouShareTheProofOfPurchase,
    TheRateIsHighCanYouExplainWhyAndShareProof,
    Other,
    Unspecified
}

/// Source of job billing submit 
public enum JobBillingSubmittedBySource
{
    InternalApi,
    AutoSubmitSystemAction,
    Operations,
    ProviderUserAction,
    TakeControlTaskAction,
    ComplaintJobInvoicingSystemAction, // what is this?
    Unspecified
}

/// Credit card provider
public enum CreditCardProvider 
{
    Unspecified,
    CorporateCard,
    Stripe,
    PncBank
}

/// Ticket stage source a provider billing visit
public enum ProviderBillingVisitTicketStageSource 
{
    Fulfillment,
    ProviderBilling
}

/// Item type of a provider billing "service line item"
public enum ProviderBillingServiceLineItemType 
{
    PerService,
    PerEvent
}

/// Item type of a provider billing "billing line item"
public enum ProviderBillingBillingLineItemType 
{
    PerEventCalculated,
    Season,
    Discount,
    ProcessingFee
}

/// Status of a provider billing's review/verification
public enum ProviderBillingStatus 
{
    // job billing specific
    Unspecified,
    Todo,
    InProgress,
    Verified,
    // provider billing only
    WaitingOnData,
    WaitingDmg,
    WaitingProvider,
    Approved,
    Cancelled,
    NoPay
}

/// Costing scheme of job billing
public enum JobBillingCostingScheme 
{
    TimeAndMaterial,
    FlatRate,
    ServiceBased
}

/// Costing scheme of provider billing
public enum ProviderBillingCostingScheme 
{
    TimeAndMaterial,
    FlatRate,
    ServiceBased,
    PerOccurrence,
    Seasonal,
    NonRoutine
}