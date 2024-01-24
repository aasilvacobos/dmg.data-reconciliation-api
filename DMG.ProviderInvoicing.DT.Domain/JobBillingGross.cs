using DMG.ProviderInvoicing.BL.Utility;
using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain;

/// Transactional Data **AGGREGATE**: Job Billing Gross
/// Job Billing Gross is the proto provider invoice as persisted in the Fulfillment system. It is created directly from the
/// job billing message without dependence on other messages.  It is used to deterministically create the
/// Job Billing (Decorated), which is presented to the provider during the billing review/verification process.
/// Job Billing Gross and Job Billing (Decorated) have similar data but a different structure.
public record JobBillingGross(
    JobBillingId                                            JobBillingId,
    JobWorkId                                               JobWorkId,
    JobBillingVersion                                       Version,
    JobBillingAssignee                                      Assignee,
    JobBillingCostingScheme                                 CostingScheme,
    decimal                                                 TotalCost,
    RecordMeta                                              RecordMeta,     // TODO move to required sections
    // optional scalars
    Option<NonEmptyText>                                    JobSummary,
    Option<NonEmptyText>                                    ProviderInvoiceNumber,
    Option<DateTimeOffset>                                  ProviderSubmittedOnDate,    // TODO remove in future since Fulfillment plans to deprecate
    Option<DateTimeOffset>                                  ProviderFirstSubmittedOnDate,
    Option<DateTimeOffset>                                  ProviderLastSubmittedOnDate,    
    // required sections
    JobBillingGrossLabor                                    Labor,
    JobBillingGrossMaterialPart                             MaterialPart,
    JobBillingGrossEquipment                                Equipment,
    JobBillingJobFlatRate                                   JobFlatRate,
    JobBillingGrossMaterialPartFlatRate                     MaterialPartFlatRate,
    JobBillingGrossEquipmentFlatRate                        EquipmentFlatRate,
    JobBillingPayment                                       Payment,
    JobBillingStatus                                        JobBillingStatus, // TODO move above optional scalars
    // optional sections
    Option<JobBillingSubmissionDetail>                      SubmissionDetailLatest,
    Option<JobBillingAdditional>                            Additional,
    // required collections
    Lst<JobBillingGrossTripChargeLineItem>                  TripChargeLineItems, // TODO convert into section with this collection
    // required collection
    Lst<JobBillingRuleMessage>                              RuleMessages,    
    // Even after we move to folder service, we will have to keep this for backward compatibility.
    Lst<JobPhoto>                                           Photos,
    Lst<JobDocument>                                        Documents) : IJobBilling;

/// Job billing labor section
public record JobBillingGrossLabor(
    decimal                                                 TotalCost,  // TODO rename to AdjustedTotalCost or TotalCost?
    // optional scalars
    // collections
    Lst<JobBillingGrossLaborLineItem>                       LaborLineItems,
    Lst<JobBillingGrossCostDiscountLineItem>                CostDiscounts,
    Lst<JobBillingRuleMessage>                              RuleMessages);

/// A Job billing gross labor line item
public record JobBillingGrossLaborLineItem(
    LineItemId                                          Id,
    JobWorkId                                           JobWorkId,
    LaborRate                                           LaborRate,
    decimal                                             LineItemCost,
    decimal                                             AdjustedTotalBillableTimeInHours,
    long                                                TotalBillableTimeInSeconds,
    long                                                AdjustedTotalBillableTimeInSeconds,
    RateType                                            RateType,
    TechnicianType                                      TechnicianType,
    // optional scalars
    Option<JobBillingElementDispute>                    Dispute,
    // collections
    Lst<JobBillingGrossTechnicianTrip>                  TechnicianTrips,
    Lst<JobBillingGrossLaborLineItemTimeAdjustment>     TimeAdjustments,
    Lst<JobBillingRuleMessage>                          RuleMessages);

/// Shared properties across any version of a labor technician trip
public interface IJobBillingLaborTechnicianTrip
{
    public TechnicianTripId                 TechnicianTripId { get; }
    public UserId                           TechnicianUserId { get; }
    public DateTimeOffset                   CheckInDateTime  { get; }
    public DateTimeOffset                   CheckOutDateTime { get; }
    public bool                             IsPayable        { get; }
    public JobBillingElementCreationSource  CreationSource   { get; }
}
/// Instance of technician making trip to job site. Used to derive labor hours.
public record JobBillingGrossTechnicianTrip( 
    TechnicianTripId                                TechnicianTripId,
    UserId                                          TechnicianUserId, 
    DateTimeOffset                                  CheckInDateTime,
    DateTimeOffset                                  CheckOutDateTime,
    long                                            TotalBillableTimeInSeconds,
    bool                                            IsPayable,
    JobBillingGrossTechnicianTripTimeOnSiteSource   TimeOnSiteSource,
    JobBillingElementCreationSource                 CreationSource) : IJobBillingLaborTechnicianTrip;
// Source of a technician trip time on site for a job billing gross
public enum JobBillingGrossTechnicianTripTimeOnSiteSource 
{
    TimeOnSiteSourceTechnician,
    TimeOnSiteSourceBackOffice
}
/// Time adjustment to a labor line item
public record JobBillingGrossLaborLineItemTimeAdjustment(
    AdjustmentSeconds                   TimeAdjustmentInSeconds,
    TimeAdjustmentCost                  Cost,
    bool                                IsProviderConfirmed,
    JobBillingElementCreationSource     CreationSource,
    // optional scalars
    Option<NonEmptyText>                Reason,
    // required sections
    RecordMeta                          RecordMeta,
    // collections
    Lst<JobBillingRuleMessage>          RuleMessages);

/// Job billing material/part section
public record JobBillingGrossMaterialPart(
    Lst<JobBillingGrossMaterialPartEquipmentLineItem>       LineItems,
    Lst<JobBillingGrossCostDiscountLineItem>                CostDiscounts,
    Lst<JobBillingRuleMessage>                              RuleMessages);

/// Job billing equipment section
public record JobBillingGrossEquipment(
    Lst<JobBillingGrossMaterialPartEquipmentLineItem>       LineItems,
    Lst<JobBillingGrossCostDiscountLineItem>                CostDiscounts,
    Lst<JobBillingRuleMessage>                              RuleMessages);

/// A generic item (catalog or non-catalog) for a material/part or equipment line
public interface IJobBillingGrossMaterialPartEquipmentItem { }
/// Catalog item for a material/part or equipment line
public record JobBillingGrossMaterialPartEquipmentCatalogItem(
    CatalogItemId                            CatalogItemId,
    Lst<JobBillingCatalogItemRuleValue>      RuleValues) : IJobBillingGrossMaterialPartEquipmentItem;
/// Non-Catalog item for a material/part or equipment line
public record JobBillingGrossMaterialPartEquipmentNonCatalogItem(
    string                      Name) : IJobBillingGrossMaterialPartEquipmentItem;

/// A material/part/equipment line item on job billing gross
public record JobBillingGrossMaterialPartEquipmentLineItem(
    Guid                                            Id,
    decimal                                         Quantity,
    UnitOfMeasure                                   UnitType,
    decimal                                         ItemCost,
    LineItemCost                                    LineItemCost,
    MaterialPartEquipmentCatalogItemType            CatalogItemType,
    IJobBillingGrossMaterialPartEquipmentItem       Item,
    JobBillingElementCreationSource                 CreationSource,
    // optional scalars
    Option<JobWorkId>                               JobWorkId,
    Option<NonEmptyText>                            Reason,  
    // required sections
    RecordMeta                                      RecordMeta,    
    // optional sections
    Option<JobBillingElementDispute>                Dispute,
    // collections
    Lst<JobBillingRuleMessage>                      RuleMessages);

/// A message on the trip charge line.
public record JobBillingRuleMessage(
    JobBillingMessageType                         MessageType,    
    JobBillingMessageRule                         MessageRule,
    // optional scalars
    Option<NonEmptyText>                          MessageText,
    // collections
    Lst<JobBillingMessageVisibility>              Visibilities);

/// A trip charge line item on the job billing gross
public record JobBillingGrossTripChargeLineItem(
    Guid                                                Id,
    WorkVisitId                                         WorkVisitId,
    RateType                                            RateType,
    TripChargeRate                                      TripChargeRate,
    LineItemCost                                        LineItemCost,
    DateTimeOffset                                      FirstTechnicianArrivalDateTime,
    DateTimeOffset                                      RequestedByDateTime,    
    bool                                                IsTripPayable,
    JobBillingElementCreationSource                     CreationSource,
    // required collections
    Lst<JobBillingRuleMessage>                          RuleMessages);

/// Material/part flat rate section on job billing gross
public record JobBillingGrossMaterialPartFlatRate(
    decimal                                                     AdjustedTotalCost,
    // required collections
    Lst<JobBillingGrossMaterialPartEquipmentFlatRateLineItem>   LineItems,
    Lst<IJobBillingCostDiscountLineItem>                        CostDiscounts,
    Lst<JobBillingRuleMessage>                                  RuleMessages);

/// Equipment flat rate section on a job billing gross
public record JobBillingGrossEquipmentFlatRate(
    decimal                                                     AdjustedTotalCost,
    // required collections
    Lst<JobBillingGrossMaterialPartEquipmentFlatRateLineItem>   LineItems,
    Lst<IJobBillingCostDiscountLineItem>                        CostDiscounts,
    Lst<JobBillingRuleMessage>                                  RuleMessages);

/// A material/part/equipment flat rate line item on job billing gross
public record JobBillingGrossMaterialPartEquipmentFlatRateLineItem(
    LineItemId                                  Id,
    decimal                                     Quantity,
    decimal                                     Rate,
    decimal                                     LineItemCost,
    CatalogItemId                               CatalogItemId,
    MaterialPartEquipmentCatalogItemType        CatalogItemType,
    bool                                        IsItemPayable,
    bool                                        IsFlaggedBySystem,
    bool                                        IsManuallyVerified,
    JobBillingElementCreationSource             CreationSource,
    // optionals
    Option<NonEmptyText>                        Reason,
    // required sections
    RecordMeta                                  RecordMeta,
    // optional sections
    Option<JobBillingElementDispute>            Dispute,
    // required collections
    Lst<JobBillingRuleMessage>                  RuleMessages);

/// Cost discount to a job billing gross section
public record JobBillingGrossCostDiscountLineItem(
    decimal                             CostDiscount,
    JobBillingElementCreationSource     CreationSource,
    RecordMeta                          RecordMeta) : IJobBillingCostDiscountLineItem;

/// Transactional Data **AGGREGATE**: Review ("raise request") associated to a job billing
public record JobBillingReview(
    JobBillingReviewId                              JobBillingReviewId,
    Lst<JobBillingReviewConversation>               Conversations);
public record JobBillingReviewConversation(
    ReviewConversationId                            ReviewConversionId,
    long                                            Order,
    JobBillingReviewRequestMessage                  RequestMessage,
    Option<JobBillingReviewResponseMessage>         ResponseMessage,
    RecordMeta                                      RecordMeta);
public record JobBillingReviewRequestMessage(
    UserId                                          RequestedByUserId,
    DateTimeOffset                                  RequestedOnDateTime,
    NonEmptyText                                    Message);
public record JobBillingReviewResponseMessage(
    UserId                                          RespondedByUserId,
    DateTimeOffset                                  RespondedOnDateTime,
    NonEmptyText                                    Message);

/// Transactional Data **AGGREGATE**: Dispute for a job billing element. 
public record JobBillingElementDispute(
    JobBillingDisputeId                                 JobBillingDisputeId,
    Lst<JobBillingElementDisputeConversation>           Conversations);
public record JobBillingElementDisputeConversation(
    DisputeConversationId                               DisputeConversionId,
    long                                                Order,
    JobBillingElementDisputeRequest                     DisputeRequest,
    RecordMeta                                          RecordMeta,
    // optionals
    Option<JobBillingElementDisputeResponse>            DisputeResponse);
public record JobBillingElementDisputeRequest(
    JobBillingLineItemDisputeRequestReason              Reason,
    RecordMeta                                          RecordMeta,
    // optionals
    Option<NonEmptyText>                                ReasonText,
    Option<NonEmptyText>                                AdditionalText);
public record JobBillingElementDisputeResponse(
    NonEmptyText                                        Message,
    RecordMeta                                          RecordMeta);

public record JobBillingSubmissionDetail(
    DateTimeOffset                                      SubmittedOnDate,
    JobBillingSubmittedBySource                         SubmittedBySource);

public record JobBillingAdditional(
    Lst<JobBillingCatalogItemEffectiveRule>             CatalogItemEffectiveRules);
public record JobBillingCatalogItemEffectiveRule(
    CatalogItemId                                       CatalogItemId,
    // optionals
    // use string as key since it must implement IComparable; may implement IComparable on NonEmptyText later
    Option<Map<string, Option<NonEmptyText>>>           RulesMap); 
public record JobBillingCatalogItemRuleValue(
    NonEmptyText                                        RuleName,
    // optionals
    Option<NonEmptyText>                                RuleValue);

/// Transactional Data **AGGREGATE**: A Job photo
public record JobPhoto( 
    JobPhotoBase                Base,      
    RecordMeta                  RecordMeta);    // deprecated
/// Base attributes used in all version of job photo entity
public record JobPhotoBase(
    JobWorkId                   JobWorkId,
    // Holds both the PK and ID of the file in the large object store
    JobPhotoId                  JobPhotoId,
    NonEmptyText                MimeType,
    PhotoChronology             PhotoChronology,
    // optionals
    Option<NonEmptyText>        FileName,
    Option<NonEmptyText>        Description,
    Option<ServiceLineId>       ServiceLineId,
    Option<VisitId>             VisitId);

/// Transactional Data **AGGREGATE**: A Job document
public record JobDocument(
    JobDocumentBase             Base,
    RecordMeta                  RecordMeta);       
/// Base attributes used in all versions of job document entity
public record JobDocumentBase(
    JobWorkId                   JobWorkId,
    // Holds both the PK and ID of the file in the large object store
    JobDocumentId               JobDocumentId,
    NonEmptyText                MimeType,
    // optionals
    Option<NonEmptyText>        FileName,
    Option<NonEmptyText>        Description);