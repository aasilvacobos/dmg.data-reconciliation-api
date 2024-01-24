using LanguageExt;
using static LanguageExt.Prelude;
using DMG.ProviderInvoicing.BL.Utility;

namespace DMG.ProviderInvoicing.DT.Domain;

// The purpose of this file is to model the provider invoicing domain specification,
// serving as both documentation and code.
// It should be free of any implementation. There should be no/ direct dependencies
// on database model, database category (SQL vs NoSQL), UI, APIs or external I/O sources.

// Entity records defined below represent entities that are either already persisted
// (externally or local database), or are a valid candidate for persistence. These records
// can also be used for a retrieve/get operations, either directly or via composition.
// In short, a record below represents a complete and validated domain entity. 

// There will be a designation when an entity is an "aggregate". An aggregate represents:
//     a) an atomic unit of persistence, and
//     b) a "consistency boundary" where the root (top-level) entity maintains the aggregate's consistency

// Many entity attributes will have a type that is specifically created for that attribute, often
// with an identical name to the attribute. For example, the attribute "JobWorkId" is typed as "JobWorkId";
// any attribute type ending "Id" simply wraps a GUID/UUID value. This type granularity approach
// provides advantages like improved compile-time resolution and avoiding null reference exceptions.
// Use of these custom types will be prioritized for text and id/identifier attributes.

// All entity names should be in the singular. The plural name for an entity should only be used 
// when it is represents a collection/list. 
 
// If an attribute is a boolean type (true/false) it should be named with the "Is" prefix within this model.
 
// ** This code should be kept as readable as possible for non-technical viewers.

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// Job Billing Decorated is a job billing that is decorated with additional data only used while the provider and OC
/// are verifying the job billing. Decorations are photo counts, review, disputes, and technician trips. 
/// These are all "shed" after a job billing is verified; they are neither persisted in the PI database nor used
/// to generate the provider invoice. In the future, it is hoped that review and disputes will be persisted
/// independently of the job billing entity within the Fulfillment system.
public record JobBillingDecorated(   
    JobBillingId                                            JobBillingId,
    JobWorkId                                               JobWorkId,              // TODO remove since it will be found in Jobs fields
    JobBillingVersion                                       Version,
    BillingContractType                                     ContractType,
    TicketId                                                TicketId,               // temporarily hydrated from job until added to job billing message
    ProviderOrgId                                           ProviderOrgId,          // temporarily hydrated from job until added to job billing message
    CustomerId                                              CustomerId,             // temporarily hydrated from job until added to job billing message
    PropertyId                                              PropertyId,
    ServiceLineId                                           ServiceLineId,          // temporarily hydrated from job until added to job billing message
    ServiceTypeId                                           ServiceTypeId,          // TODO remove since it will be found in Jobs fields
    NonEmptyText                                            JobWorkNumber,          // TODO remove since it will be found in Jobs fields
    NonEmptyText                                            TicketNumber,           // temporarily hydrated from job until added to job billing message
    DateTimeOffset                                          JobCompleteDateTime,    // TODO remove since it will be found in Jobs fields
    JobBillingCostingScheme                                 CostingScheme,
    JobBillingStatus                                        JobBillingStatus,
    decimal                                                 TotalCost,
    JobBillingAssignee                                      Assignee,
    // optional scalars
    Option<NonEmptyText>                                    JobSummary,
    Option<NonEmptyText>                                    ProviderInvoiceNumber,  
    Option<DateTimeOffset>                                  ProviderSubmittedOnDate,        // TODO remove in future since Fulfillment plans to deprecate
    Option<DateTimeOffset>                                  ProviderFirstSubmittedOnDate,
    Option<DateTimeOffset>                                  ProviderLastSubmittedOnDate,    
    // required sections
    RecordMeta                                              RecordMeta,
    JobBillingDecoratedMaterialPart                         MaterialPart,
    JobBillingDecoratedEquipment                            Equipment,
    JobBillingDecoratedLabor                                Labor,
    JobBillingTripCharge                                    TripCharge,     // use standard job billing structure since it is identical the decorated      
    JobBillingJobFlatRate                                   JobFlatRate,
    JobBillingDecoratedMaterialPartFlatRate                 MaterialPartFlatRate,
    JobBillingDecoratedEquipmentFlatRate                    EquipmentFlatRate,
    JobBillingProcessingFee                                 ProcessingFee,
    JobBillingPayment                                       Payment,
    JobBillingJobGroup                                      Job,                    // Represents "event" in routine
    PhotoFolder                                             PhotoFolder,            // TODO remove since it will be found in Jobs fields
    // optional sections
    Option<JobBillingSubmissionDetail>                      SubmissionDetailLatest,
    Option<JobBillingAdditional>                            Additional,
    // required collection
    Lst<JobBillingRuleMessage>                              RuleMessages) : IJobBilling; 

/// Job billing decorated material/part section
public record JobBillingDecoratedMaterialPart(
    // collections
    Lst<JobBillingDecoratedMaterialPartLineItem>            LineItems,
    Lst<IJobBillingCostDiscountLineItem>                    CostDiscounts,
    Lst<JobBillingRuleMessage>                              RuleMessages);

/// A job billing decorated material/part line item
public record JobBillingDecoratedMaterialPartLineItem(
    JobBillingMaterialPartLineItem                          Core,
    // optionals
    Option<JobBillingElementDispute>                        Dispute);

/// Job billing decorated equipment section
public record JobBillingDecoratedEquipment(
    // collections
    Lst<JobBillingDecoratedEquipmentLineItem>               LineItems,
    Lst<IJobBillingCostDiscountLineItem>                    CostDiscounts,
    Lst<JobBillingRuleMessage>                              RuleMessages);

/// A job billing decorated equipment line item
public record JobBillingDecoratedEquipmentLineItem(
    JobBillingEquipmentLineItem                             Core,
    // optionals
    Option<JobBillingElementDispute>                        Dispute);

/// Job billing decorated labor section
public record JobBillingDecoratedLabor(
    decimal                                                 TotalCost,      // TODO rename to AdjustedTotalCost
    // collections
    Lst<IJobBillingDecoratedLaborLineItem>                  LineItems,
    Lst<IJobBillingCostDiscountLineItem>                    CostDiscounts,
    Lst<JobBillingRuleMessage>                              RuleMessages) : IJobBillingLabor<IJobBillingDecoratedLaborLineItem>;

public interface IJobBillingDecoratedLaborLineItem : IJobBillingLaborLineItem 
{
    public long                                                 TotalBillableTimeInSeconds         { get; }
    public long                                                 AdjustedTotalBillableTimeInSeconds { get; }
    // optionals
    public Option<JobBillingElementDispute>                     Dispute                            { get; }
    // collections
    public Lst<JobBillingDecoratedLaborLineItemTechnicianTrip>  TechnicianTypeTrips                { get; } 
}

/// A job billing decorated retrieve labor item
public record JobBillingDecoratedLaborLineItem(
    LineItemId                                                      Id,
    JobWorkId                                                       JobWorkId,
    TechnicianType                                                  TechnicianType,
    decimal                                                         AdjustedTotalBillableTimeInHours,
    long                                                            TotalBillableTimeInSeconds,
    long                                                            AdjustedTotalBillableTimeInSeconds,    
    LaborRate                                                       LaborRate,          // temporarily hydrated from job until added to job billing message 
    decimal                                                         LineItemCost,       // TODO rename to AdjustedLineItemCost
    RateType                                                        RateType,
    // optionals
    Option<JobBillingElementDispute>                                Dispute,
    // detail collections
    Lst<IJobBillingLaborLineItemTimeAdjustment>                     TimeAdjustments,
    Lst<JobBillingDecoratedLaborLineItemTechnicianTrip>             TechnicianTypeTrips,
    Lst<JobBillingRuleMessage>                                      RuleMessages) : IJobBillingDecoratedLaborLineItem;

/// A technician trip associated to job billing decorated labor item
public record JobBillingDecoratedLaborLineItemTechnicianTrip(
    TechnicianTripId                                            TechnicianTripId,
    UserId                                                      TechnicianUserId,
    DateTimeOffset                                              CheckInDateTime,
    DateTimeOffset                                              CheckOutDateTime,
    long                                                        TotalBillableTimeInSeconds,
    bool                                                        IsPayable,
    JobBillingElementCreationSource                             CreationSource) : IJobBillingLaborTechnicianTrip;

/// Job billing decorated material/part flat rate section
public record JobBillingDecoratedMaterialPartFlatRate(
    decimal                                                 AdjustedTotalCost,
    // required collections
    Lst<JobBillingDecoratedMaterialPartFlatRateLineItem>    LineItems,
    Lst<IJobBillingCostDiscountLineItem>                    CostDiscounts,
    Lst<JobBillingRuleMessage>                              Messages);

/// A job billing decorated material/part flat rate line item
public record JobBillingDecoratedMaterialPartFlatRateLineItem(
    JobBillingMaterialPartFlatRateLineItem                  Core,
    // optionals
    Option<JobBillingElementDispute>                        Dispute);

/// Job billing decorated equipment flat rate section
public record JobBillingDecoratedEquipmentFlatRate(
    decimal                                                 AdjustedTotalCost,
    // required collections
    Lst<JobBillingDecoratedEquipmentFlatRateLineItem>       LineItems,
    Lst<IJobBillingCostDiscountLineItem>                    CostDiscounts,
    Lst<JobBillingRuleMessage>                              Messages);

/// A job billing decorated equipment flat rate line item
public record JobBillingDecoratedEquipmentFlatRateLineItem(
    JobBillingEquipmentFlatRateLineItem                     Core,
    // optionals
    Option<JobBillingElementDispute>                        Dispute);

/// Transactional Data **AGGREGATE**: A photo folder (expected to be used in future service)
public record PhotoFolder(
    PhotoBeforeCount        BeforePhotosCount,
    PhotoAfterCount         AfterPhotosCount);    