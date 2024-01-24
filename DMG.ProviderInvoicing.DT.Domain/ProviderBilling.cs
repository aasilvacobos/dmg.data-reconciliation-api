using LanguageExt;
using static LanguageExt.Prelude;
using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Domain.Rule;

namespace DMG.ProviderInvoicing.DT.Domain;

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// Provider Billing is the entity that supersedes the job billing from Fulfillment. It is both routine and
/// non-routine capable, where the job billing handles only non-routine. A provider billing contains only
/// one of the original "decorations" found on the job billing: disputes.
/// 
/// The provider billing and job billing share significant amount of structure, and thus there are records
/// prefixed with "JobBilling" that are used to compose the ProviderBilling. Eventually, these prefixes can
/// be renamed to "Billing".
/// 
/// This entity represents a provider billing that has already been persisted in the provider billing SOR.
/// It should only be used for retrieval for mapping from the job billing.
public record ProviderBilling(   
    ProviderBillingId                           ProviderBillingId,
    JobBillingVersion                           Version,
    bool                                        IsCIWO,
    BillingContractType                         ContractType,
    TicketId                                    TicketId,
    NonEmptyText                                TicketNumber,
    ProviderOrgId                               ProviderOrgId,
    CustomerId                                  CustomerId,
    PropertyId                                  PropertyId,             
    ServiceLineId                               ServiceLineId,
    ProviderBillingCostingScheme                CostingScheme,
    ProviderBillingStatus                       ProviderBillingStatus,
    decimal                                     TotalCost,
    ProviderBillingAssignee                     Assignee,
    SourceSystem                                SourceSystem,
    BillingType                                 BillingType,
    // optional scalars
    Option<JobBillingId>                        JobBillingId,                   // The job billing id represents a job billing in Fulfillment. It should only have a value if it is Fulfillment Non-Routine. 
    Option<NonEmptyText>                        ProviderBillingNumber,          // None for job billing
    Option<NonEmptyText>                        JobSummary,
    Option<NonEmptyText>                        Detail,                         // Routine only. A description entered by DMG user; analogue to non-routine job summary. Equivalent to "Event Details" (expecting "event" naming to not be used) 
    Option<NonEmptyText>                        Note,                           // Routine only. Entered by provider; no analogue to non-routine 
    Option<NonEmptyText>                        ProviderInvoiceNumber,  
    Option<DateTimeOffset>                      ProviderFirstSubmittedOnDate,
    Option<DateTimeOffset>                      ProviderLastSubmittedOnDate,    
    Option<DateTimeOffset>                      InvoiceCreationDate,
    Option<PsaId>                               PsaId,
    // required sections
    RecordMeta                                  RecordMeta,
    JobBillingDecoratedMaterialPart             MaterialPart,
    JobBillingDecoratedEquipment                Equipment,
    JobBillingDecoratedLabor                    Labor,
    JobBillingTripCharge                        TripCharge,      
    JobBillingJobFlatRate                       JobFlatRate,
    JobBillingDecoratedMaterialPartFlatRate     MaterialPartFlatRate,
    JobBillingDecoratedEquipmentFlatRate        EquipmentFlatRate,
    ProviderBillingVisitDetail                  Visit,
    ProviderBillingBillingDetail                Billing,
    JobBillingProcessingFee                     ProcessingFee,
    JobBillingPayment                           Payment,
    ProviderBillingJobGroup                     Job,                            // Represents "event" for routine
    ProviderBillingDiscount                     Discount,
    // optional sections
    Option<JobBillingSubmissionDetail>          SubmissionDetailLatest,
    Option<JobBillingAdditional>                Additional,
    Option<Event>                               Event,
    Option<ProviderBillingTripChargeLineItem>   ProviderBillingTripChargeLineItem,
    Option<WeatherWorks>                        WeatherWorks,
    // required collection
    Lst<JobBillingRuleMessage>                  RuleMessages,
    Lst<MultiVisitJobRate>                      MultiVisitJobRates);

public record Event(
    EventId                                     EventId,
    ServiceItemId                               ServiceItemId,
    NonEmptyText                                Name,
    EventGroupSource                            Source,
    decimal                                     Amount,
    DateTimeOffset                              EventStart,
    DateTimeOffset                              EventEnd,
    SourceId                                    SourceId,
    
    // optional sections
    Option<NonEmptyText>                        Description,
    Option<EventLineItemId>                     EventLineItemId,
    Option<ModifyBy>                            ModifyBy,
    Option<ModifyAction>                        ModifyAction,
    Option<NonEmptyText>                        Reason);

public record ProviderBillingDiscount(
    decimal                                     DistrictManagerDiscount,
    decimal                                     ProviderDiscount,
    Option<Guid>                                ProviderDiscountId);

public record ProviderBillingVisitDetail(
    Lst<ProviderBillingVisit>                   Visits);

public record ProviderBillingVisit(
    // required scalars
    VisitId                                     VisitId,
    DateTimeOffset                              CheckInDateTime,
    DateTimeOffset                              CheckOutDateTime,
    ProviderBillingVisitTicketStageSource       TicketStageSource,
    bool                                        MissedCheckIn,
    bool                                        Addendum,
    Option<NonEmptyText>                        WeatherWorksDescription,
    Option<JobWorkId>                           JobWorkId,
    // required collections
    Lst<ProviderBillingServiceLineItem>         ServiceLineItems,
    Lst<ProviderBillingTimeAndMaterialLineItem> TimeAndMaterialLineItems);

public record ProviderBillingServiceLineItem(
    LineItemId                                  Id,
    Option<NonEmptyText>                        Name,
    DateTimeOffset                              VisitDateTime,
    ServiceRate                                 ServiceRate,
    decimal                                     Quantity,
    LineItemCost                                LineItemCost,
    ProviderBillingServiceLineItemType          ItemType,
    ServiceTypeId                               ServiceTypeId,
    PerOccurrenceItemId                         PerOccurrenceItemId,
    ModifyBy                                    ModifyBy,
    ModifyAction                                ModifyAction,
    // optional scalars
    Option<NonEmptyText>                        Reason,
    // optional sections
    Option<JobBillingElementDispute>            Dispute,
    Option<VisitServiceLineItemAdjustment>      Adjustment);

public record ProviderBillingTimeAndMaterialLineItem(
    VisitId                                     VisitId,
    Option<NonEmptyText>                        Reason,
    ServiceItemId                               ServiceItemId,
    LineItemId                                  ItemId,
    DateTimeOffset                              ClientTime,
    int                                         ItemSequenceNumber,
    ProviderBillingSource                       Source,
    decimal                                     Quantity,
    decimal                                     Amount,
    ServiceItemSource                           ServiceItemSource,
    UnitOfMeasure                               UnitTypeMeasurement,
    decimal                                     Rate,
    string                                      RateUnitType,
    ModifyBy                                    ModifyBy,
    ModifyAction                                ModifyAction,
    Option<TimeAndMaterialLineItemAdjustment>   Adjustment);

public record ProviderBillingTripChargeLineItem(
    LineItemId                                  LineItemId,
    NonEmptyText                                Name,
    decimal                                     Rate,
    decimal                                     Amount,
    decimal                                     Quantity,
    int                                         MaximumChargeableTrips,
    ProviderBillingSource                       Source);

public record WeatherWorks(
    decimal                                     Snow,
    decimal                                     Ice,
    Option<decimal>                             TemperatureInFahrenheit,
    Option<NonEmptyText>                        Description
    );

public record TimeAndMaterialLineItemAdjustment(
    LineItemId                                  LineItemId,
    decimal                                     Quantity,
    decimal                                     Amount
    );

public record VisitServiceLineItemAdjustment(
    Option<NonEmptyText>                        Name,
    LineItemId                                  LineItemId,
    decimal                                     Amount
    );

public record ProviderBillingBillingDetail(
    Lst<ProviderBillingBillingLineItem>         LineItems);

public record ProviderBillingBillingLineItem(
    // required scalars
    LineItemId                                  Id,
    NonEmptyText                                Name,
    ProviderBillingBillingLineItemType          ItemType,
    decimal                                     Price,
    decimal                                     Quantity);

public record ProviderBillingJobGroup(
    // required collections
    Lst<ProviderBillingJob>                     Jobs);

/// A job on a job billing. All fields (except AttachmentMeta, JobCondition, and Costings) are immutable at the time job is added to group.
public record ProviderBillingJob(
    JobWorkId                                   JobWorkId,
    ServiceTypeId                               ServiceTypeId,
    NonEmptyText                                JobWorkNumber,
    JobUrgency                                  Urgency,
    Option<DateTimeOffset>                      JobCompleteDate,
    NonEmptyText                                Scope,
    // optional scalars
    Option<JobWorkState>                        JobWorkState,   // Temporarily used to pass through from job billing provided by Fulfillment. For provider billing, set to None. Should be removed once we no longer use Fulfillment for job billing.
    Option<JobWorkStatus>                       JobWorkStatus,  // Temporarily used to pass through from job billing provided by Fulfillment. For provider billing, set to None. Should be removed once we no longer use Fulfillment for job billing.
    // required sections
    BillingJobAttachmentMeta                    AttachmentMeta, // Used to determine chronology counts until we move to file service
    // optional sections
    Option<JobCondition>                        JobCondition,   // Temporarily used to pass through from job billing provided by Fulfillment. For provider billing, set to None. Should be removed once we no longer use Fulfillment for job billing.
    // optional collections
    Option<Lst<Costing>>                        Costings)       // Temporarily used to pass through from job billing provided by Fulfillment. For provider billing, set to None. Should be removed once we no longer use Fulfillment for job billing.
{
    // Computed fields
    public Option<bool> IsProviderNotResponding => this.JobWorkState.Map(JobRule.IsProviderNotResponding);
};
    
/// Meta data for attachments (photo, documents) on a job grouped on a provider billing
public record BillingJobAttachmentMeta(
    PhotoBeforeCount                           BeforePhotosCount,
    PhotoAfterCount                            AfterPhotosCount);        
    
    public record DeletePhoto(
    JobPhotoId                                  jobPhotoId);
    public record DeleteVisit(
    VisitId                                     visitId);

    public record DeleteLineItem(
    LineItemId                                  lineItemId,
    NonEmptyText                                reason);    
    public record UpdateVisit(
    VisitId                                     visitId,
    DateTimeOffset                              checkInTime,
    DateTimeOffset                              checkOutTime);
    public record UpdateDiscount(
    LineItemId                                  lineItemId,
    decimal                                     amount,
    NonEmptyText                                reason);

public record UpdateServiceLineItem(
    LineItemId                                  lineItemId,
    NonEmptyText                                reason,
    DateTimeOffset                              updatedTime);

public record MultiVisitJobRate(
    MultiJobVisitId                             MultiVisitJobId,
    ProviderBillingId                           ProviderBillingId,
    ServiceBasedCostingId                       ServiceBasedCostingId,
    NonEmptyText                                Name,
    decimal                                     Rate,
    UnitOfMeasure                               UnitType
);
