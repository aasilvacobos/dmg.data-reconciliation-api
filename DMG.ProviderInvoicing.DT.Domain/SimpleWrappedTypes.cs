namespace DMG.ProviderInvoicing.DT.Domain;

// Contains attribute-specific types in external domain. The Value field is used to 
// unwrap/extract the value.

/////////////////////////////////////////////////////////////////////////////////
// Guid Id types. We are currently assuming empty Guids will never occur and thus
// not validating for this case.

/// Domain type for job work id
public record JobWorkId(Guid Value);
/// Domain type for job costing id
public record JobCostingId(Guid Value);
/// Domain type for work visit id
public record WorkVisitId(Guid Value);
/// Domain type for job billing id
public record JobBillingId(Guid Value);
/// Domain type for a line item id (on job billing, provider invoice, et. al)
public record LineItemId(Guid Value);
/// Domain type for a technician trip id (on job billing)
public record TechnicianTripId(Guid Value);
/// Domain type for job photo id
public record JobPhotoId(Guid Value);
/// Domain type for job document id
public record JobDocumentId(Guid Value);
/// Domain type for review id
public record JobBillingReviewId(Guid Value);
/// Domain type for review conversation id
public record ReviewConversationId(Guid Value);
/// Domain type for dispute id
public record JobBillingDisputeId(Guid Value);
/// Domain type for dispute conversation id
public record DisputeConversationId(Guid Value);
/// Domain type for customer id
public record CustomerId(Guid Value);
/// Domain type for logo photo id
public record LogoPhotoId(Guid Value);
/// Domain type for provider org id
public record ProviderOrgId(Guid Value);
/// Domain type for provider org address id
public record ProviderOrgAddressId(Guid Value);
/// Domain type for ticket id
public record TicketId(Guid Value);
/// Domain type for ticket billing id
public record TicketBillingId(Guid Value);
/// Domain type for Customer invoice external id
public record CustomerInvoiceExternalId(Guid Value);
/// Domain type for contract term sheet id
public record ContractTermSheetId(Guid Value);
/// Domain type for master contract term sheet id
public record MasterContractTermSheetId(Guid Value);
/// Domain type for property id
public record PropertyId(Guid Value);
/// Domain type for address id
public record AddressId(Guid Value);
/// Domain type for service line id
public record ServiceLineId(Guid Value);
/// Domain type for per occurrence item id
public record PerOccurrenceItemId(Guid Value);
/// Domain type for user id
public record ServiceTypeId(Guid Value);
/// Domain type for user id
public record UserId(Guid Value);
/// Domain type for task id
public record TaskId(Guid Value);
/// Domain type for catalog item id
public record CatalogItemId(Guid Value);
/// Domain type for catalog item availability range id
public record CatalogItemAvailabilityRangeId(Guid Value);
/// Domain type for geo coverage id
public record GeoCoverageId(Guid Value);
/// Domain type for lookup item id
public record LookupItemId(Guid Value);
/// Domain type for provider invoice id
public record ProviderInvoiceId(Guid Value);
/// Domain type for a flat rate item id (on costing)
public record FlatRateCostingSchemeItemId(Guid Value);
/// Domain type an event id
public record EventId(Guid Value);
/// Domain type an psa id
public record PsaId(Guid Value);
/// Invoice Event Item Id
public record InvoiceEventItemId(Guid Value);
/// Domain type for provider billing id. Synonymous with JobBillingId
public record ProviderBillingId(Guid Value);
/// Domain type for visit id.
public record VisitId(Guid Value);
/// Domain type for MultiJob visit id.
public record MultiJobVisitId(Guid Value);
/// Domain type for MultiJob visit Row id.
public record MultiJobVisitRowId(Guid Value);
/// Domain type for Service Based Costing Id.
public record  ServiceBasedCostingId(Guid Value);
/// Domain type for event line item id.
public record EventLineItemId(Guid Value);
/// Domain type for service item id.
public record ServiceItemId(Guid Value);
/// Domain type for source id.
public record SourceId(Guid Value);
/////////////////////////////////////////////////////////////////////////////////
// Decimal types

/// Domain type for labor rate, both regular and helper
public record LaborRate(decimal Value);
/// Domain type for trip charge rate
public record TripChargeRate(decimal Value);
/// Domain type for a service rate
public record ServiceRate(decimal Value);
/// Domain type for provider not-to-exceed amount
public record ProviderNotToExceedAmount(decimal Value);
/// Domain type for item cost
public record ItemCost(decimal Value);
/// Domain type for line item cost (e.g., item cost * quantity)
public record LineItemCost(decimal Value);
/// Domain type for the cost of a labor time adjustment
public record TimeAdjustmentCost(decimal Value);
/// Domain type for a processing fee
public record ProcessingFee(decimal Value);
/// Domain type for a line processing fee
public record LineProcessingFee(decimal Value);
/// Domain type for a payment amount
public record PaymentAmount(decimal Value);
/// Domain type for a event amount
public record EventAmount (decimal Value);  

/////////////////////////////////////////////////////////////////////////////////
// Float types

/// Domain type for a variance value
public record VarianceValue(decimal Value); 

/////////////////////////////////////////////////////////////////////////////////
// Integer types

/// Domain type for a job billing's version
public record JobBillingVersion(uint Value);
/// Domain type for a lookup data set id
public record LookupDataSetId(int Value);
/// Domain type for the before chronology count for set of photos
public record PhotoBeforeCount(int Value);
/// Domain type for the after chronology count for set of photos
public record PhotoAfterCount(int Value);
/// Domain type for adjustment in minutes
public record AdjustmentMinutes(int Value);
/// Domain type for adjustment in seconds
public record AdjustmentSeconds(int Value);
/// Domain type for feature flag id
public record FeatureFlagId(int Value);


/////////////////////////////////////////////////////////////////////////////////
// Uint types

/// Domain type for payment terms identifier
public record PaymentTermsIdentifier(uint Value);

/////////////////////////////////////////////////////////////////////////////////
// Boolean types


/////////////////////////////////////////////////////////////////////////////////
// Long types
/// Domain type for provider billings version
public record ProviderBillingVersion (long Value);
