using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain;

public enum JobConditionAdditionalRule 
{
    NoBeforePhotosAllowed,
    NoAfterPhotosAllowed,
    NoDuringPhotosAllowed,
    NotToExceedAmountIncreaseNotAllowed,
    CheckOutQuestionnaireApplicable,
    SameCheckoutExperience,
    InspectionApplicable,
    Undefined,
    EventGroupingDisabled,
    DiwoEnabled,
    BillingAssignmentPropertyDm,
    CiwoEnabled,
    DoesNotOverrideExistingMvj,
    ExternalCheckInCheckOutNotAllowed
}

/// Transactional Data **AGGREGATE**: The condition data for a job.
public record JobCondition(
    // PropertyId and ServiceTypeId are the primary key
    PropertyId                                  PropertyId,
    ServiceTypeId                               ServiceTypeId,
    bool                                        IsTechnicianTripCheckInRequired,
    bool                                        IsProviderOnSamePropertyOnSameDay,
    uint                                        MinimumBeforePhotos,
    uint                                        MaximumBeforePhotos,
    uint                                        MinimumAfterPhotos,
    uint                                        MaximumAfterPhotos,
    uint                                        MinimumMinutesForTechnicianTripCheckin,
    uint                                        MaximumRegularTechniciansAllowed,
    uint                                        MaximumHelperTechniciansAllowed,
    uint                                        MaximumTripChargesAllowed,
    // collections
    Lst<CatalogItemId>                          NonBillableCatalogItemIds,
    Lst<JobConditionAdditionalRule>             AdditionalRules);