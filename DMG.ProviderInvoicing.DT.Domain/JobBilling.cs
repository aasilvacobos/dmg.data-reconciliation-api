using System;
using LanguageExt;
using static LanguageExt.Prelude;
using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Domain.Rule;

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

/// Shared properties by all job billing entities
public interface IJobBilling
{
    public JobBillingId                 JobBillingId        { get; }
    public JobWorkId                    JobWorkId           { get; }
    public JobBillingVersion            Version             { get; }
    // TODO add after Fulfillment adds to job billing message
    //public TicketId          TicketId      { get; }
    //public ProviderOrgId        ProviderOrgId    { get; }
    //public CustomerId        CustomerId    { get; }
    //public PropertyId        PropertyId    { get; }
    //public ServiceLineId     ServiceLineId { get; }
    //public ServiceTypeId     ServiceTypeId { get; }
    //public NonEmptyText         TicketNumber        { get; }
    //public NonEmptyText         JobWorkNumber       { get; }
    //public DateTimeOffset       JobCompleteDateTime { get; }
    public JobBillingCostingScheme      CostingScheme           { get; }
    public JobBillingStatus             JobBillingStatus        { get; }     
    public RecordMeta                   RecordMeta              { get; }
    // optionals
    public Option<NonEmptyText>         JobSummary              { get; }
    public Option<NonEmptyText>         ProviderInvoiceNumber   { get; }
} 

/// Required properties for all entities of type job billing labor line item
public interface IJobBillingLaborLineItem
{
    public LineItemId                                       Id                                  { get; }
    public JobWorkId                                        JobWorkId                           { get; }
    public TechnicianType                                   TechnicianType                      { get; }
    public decimal                                          AdjustedTotalBillableTimeInHours    { get; }
    public LaborRate                                        LaborRate                           { get; }
    public decimal                                          LineItemCost                        { get; }
    public RateType                                         RateType                            { get; }
    // collections
    public Lst<IJobBillingLaborLineItemTimeAdjustment>      TimeAdjustments                     { get; }
    public Lst<JobBillingRuleMessage>                       RuleMessages                        { get; }
}

///////////////////////////////////////////////////////////////////////////////////////////////////////
/// Transactional Data **AGGREGATE**: Job Billing
/// A job billing is a proto provider invoice. It is deterministically derived from both the job and job billing gross
/// (received from Fulfillment), and then serves as the input to deterministically create a provider invoice. This entity
/// is persisted in PI database. It is distinct from the "job billing decorated" which is a custom, expanded
/// version of the job billing that is required by the UI. 
public record JobBilling( 
    JobBillingId                                JobBillingId,
    JobWorkId                                   JobWorkId,              // TODO remove since it will be found in Jobs fields
    JobBillingVersion                           Version,
    BillingContractType                         ContractType,
    TicketId                                    TicketId,               // temporarily hydrated from job until added to job billing message
    ProviderOrgId                               ProviderOrgId,          // temporarily hydrated from job until added to job billing message
    CustomerId                                  CustomerId,             // temporarily hydrated from job until added to job billing message
    PropertyId                                  PropertyId,
    ServiceLineId                               ServiceLineId,          // temporarily hydrated from job until added to job billing message
    ServiceTypeId                               ServiceTypeId,          // TODO remove since it will be found in Jobs fields
    NonEmptyText                                TicketNumber,           // temporarily hydrated from job until added to job billing message
    NonEmptyText                                JobWorkNumber,          // TODO remove since it will be found in Jobs fields
    DateTimeOffset                              JobCompleteDateTime,    // TODO remove since it will be found in Jobs fields
    JobBillingCostingScheme                     CostingScheme,
    JobBillingStatus                            JobBillingStatus,
    decimal                                     TotalCost,
    JobBillingAssignee                          Assignee,
    // optionals
    Option<NonEmptyText>                        JobSummary,
    Option<NonEmptyText>                        ProviderInvoiceNumber,  
    Option<DateTimeOffset>                      ProviderSubmittedOnDate,        // TODO remove in future since Fulfillment plans to deprecate
    Option<DateTimeOffset>                      ProviderFirstSubmittedOnDate,
    Option<DateTimeOffset>                      ProviderLastSubmittedOnDate,
    // required sections
    RecordMeta                                  RecordMeta,
    JobBillingMaterialPart                      MaterialPart,
    JobBillingEquipment                         Equipment,
    JobBillingLabor                             Labor,
    JobBillingTripCharge                        TripCharge,
    JobBillingJobFlatRate                       JobFlatRate,
    JobBillingDecoratedMaterialPartFlatRate     MaterialPartFlatRate,
    JobBillingDecoratedEquipmentFlatRate        EquipmentFlatRate,
    JobBillingProcessingFee                     ProcessingFee,
    JobBillingPayment                           Payment,
    JobBillingJobGroup                          Job,                    // Represents "event" in routine
    PhotoFolder                                 PhotoFolder,            // TODO remove since it will be found in Jobs fields
    // optional sections
    Option<JobBillingSubmissionDetail>          SubmissionDetailLatest,
    Option<JobBillingAdditional>                Additional,
    // required collection
    Lst<JobBillingRuleMessage>                  RuleMessages) : IJobBilling;

/// Job section on a job billing
public record JobBillingJobGroup(
    // required collections
    Lst<JobBillingJob>                          Jobs);

/// A job on a job billing
public record JobBillingJob(
    JobWorkId                        JobWorkId,
    ServiceTypeId                    ServiceTypeId,
    NonEmptyText                     JobWorkNumber,
    JobUrgency                       Urgency,
    DateTimeOffset                   JobCompleteDate,
    NonEmptyText                     Scope,
    // optional scalars
    Option<JobWorkState>             JobWorkState,  // For job billing, Fulfillment provides. For provider billing, we do not expect to have this.
    Option<JobWorkStatus>            JobWorkStatus, // For job billing, Fulfillment provides. For provider billing, we do not expect to have this.
    // required sections
    PhotoFolder                      PhotoFolder,   // used to determine chronology counts until we move to folder service
    // optional sections
    Option<JobCondition>             JobCondition,  // For job billing, Fulfillment provides. For provider billing, use GraphQL resolver.
    // optional collections
    Option<Lst<Costing>>             Costings)      // For job billing, Fulfillment provides. For provider billing, use GraphQL resolver.  
{
    // Computed fields
    public Option<bool> IsProviderNotResponding => this.JobWorkState.Map(JobRule.IsProviderNotResponding);
}

/// Job billing material/part section
public record JobBillingMaterialPart(
    Lst<JobBillingDecoratedMaterialPartLineItem>    LineItems,
    Lst<IJobBillingCostDiscountLineItem>            CostDiscounts,
    Lst<JobBillingRuleMessage>                      RuleMessages);

/// A job billing item that is material/part or equipment
public interface IJobBillingMaterialPartEquipmentItem { }
/// A job billing item that is material/part or equipment and in the catalog
public record JobBillingMaterialPartEquipmentCatalogItem(
    CatalogItemId                           CatalogItemId, 
    Lst<JobBillingCatalogItemRuleValue>     RuleValues) : IJobBillingMaterialPartEquipmentItem;
/// A job billing item that is material/part or equipment but not in the catalog
public record JobBillingMaterialPartEquipmentNonCatalogItem(
    NonEmptyText                Name) : IJobBillingMaterialPartEquipmentItem;

/// A job billing material/part line item
public record JobBillingMaterialPartLineItem(
    Guid                                        Id,
    MaterialPartCatalogItemType                 CatalogItemType,
    IJobBillingMaterialPartEquipmentItem        Item,
    decimal                                     Quantity,
    UnitOfMeasure                               UnitType,
    ItemCost                                    ItemCost, 
    LineItemCost                                LineItemCost,
    JobBillingElementCreationSource             CreationSource,
    // optionals
    Option<JobWorkId>                           JobWorkId,
    Option<NonEmptyText>                        Reason,
    // required sections
    RecordMeta                                  RecordMeta,
    // collections
    Lst<JobBillingRuleMessage>                  RuleMessages);

/// Job billing equipment section
public record JobBillingEquipment(
    Lst<JobBillingDecoratedEquipmentLineItem>   LineItems,
    Lst<IJobBillingCostDiscountLineItem>        CostDiscounts,
    Lst<JobBillingRuleMessage>                  RuleMessages);

/// A job billing equipment line item
public record JobBillingEquipmentLineItem(
    Guid                                        Id,
    EquipmentCatalogItemType                    CatalogItemType,
    IJobBillingMaterialPartEquipmentItem        Item,
    decimal                                     Quantity,
    UnitOfMeasure                               UnitType,
    ItemCost                                    ItemCost, 
    LineItemCost                                LineItemCost,
    JobBillingElementCreationSource             CreationSource,
    // optionals
    Option<JobWorkId>                           JobWorkId,
    Option<NonEmptyText>                        Reason,
    // required sections
    RecordMeta                                  RecordMeta,
    // collections
    Lst<JobBillingRuleMessage>                  RuleMessages);

/// Interface of job billing labor section
public interface IJobBillingLabor<TIJobBillingLaborLineItem> where TIJobBillingLaborLineItem : IJobBillingLaborLineItem
{
    public Lst<TIJobBillingLaborLineItem>         LineItems       { get; } 
    public Lst<IJobBillingCostDiscountLineItem>   CostDiscounts   { get; }
};

/// Job billing labor section
public record JobBillingLabor(
    decimal                                             TotalCost,  // TODO rename to AdjustedTotalCost
    // collections
    Lst<IJobBillingDecoratedLaborLineItem>              LineItems,
    Lst<IJobBillingCostDiscountLineItem>                CostDiscounts,
    Lst<JobBillingRuleMessage>                          RuleMessages): IJobBillingLabor<IJobBillingDecoratedLaborLineItem>;

/// A job billing labor item
public record JobBillingLaborLineItem(   
    LineItemId                                      Id,
    JobWorkId                                       JobWorkId, 
    TechnicianType                                  TechnicianType,
    decimal                                         AdjustedTotalBillableTimeInHours,
    LaborRate                                       LaborRate, 
    decimal                                         LineItemCost,
    RateType                                        RateType,
    // collections
    Lst<IJobBillingLaborLineItemTimeAdjustment>     TimeAdjustments,
    Lst<JobBillingRuleMessage>                      RuleMessages) : IJobBillingLaborLineItem;

/// Required properties for all entities of type job billing labor line item time adjustment
public interface IJobBillingLaborLineItemTimeAdjustment 
{
    public AdjustmentMinutes                TimeAdjustmentInMinutes { get; }
    public TimeAdjustmentCost               Cost                    { get; }
    public bool                             IsProviderConfirmed     { get; }
    public JobBillingElementCreationSource  CreationSource          { get; }
    // optional scalars
    public Option<NonEmptyText>             Reason                  { get; }
    // required sections
    public RecordMeta                       RecordMeta              { get; }
    public Lst<JobBillingRuleMessage>       RuleMessages            { get; }
}

/// Time adjustment to a job billing section 
public record JobBillingLaborLineItemTimeAdjustment(
    AdjustmentMinutes                   TimeAdjustmentInMinutes,
    TimeAdjustmentCost                  Cost,
    bool                                IsProviderConfirmed,
    JobBillingElementCreationSource     CreationSource,
    // optional scalars
    Option<NonEmptyText>                Reason,
    // required sections
    RecordMeta                          RecordMeta,
    // collections
    Lst<JobBillingRuleMessage>          RuleMessages) : IJobBillingLaborLineItemTimeAdjustment;

/// Job billing trip charge section
public record JobBillingTripCharge(
    Lst<JobBillingTripChargeLineItem>       LineItems)
{
    // Computed fields
    public decimal TotalCost => JobBillingRule.CalculateTripChargeTotalCost(this.LineItems);
}

/// Required properties for all entities of type job billing trip charge line item
public interface IJobBillingTripChargeLineItem
{
    public NonEmptyText                     Description             { get; }
    public DateTimeOffset                   RequestedByDate         { get; }
    public DateTimeOffset                   ArrivalDate             { get; }
    public RateType                         RateType                { get; }
    public TripChargeRate                   TripChargeRate          { get; }
    public LineItemCost                     LineItemCost            { get; }
    public bool                             IsTripPayable           { get; }
    public bool                             IsRequestedByDateMissed { get; }
    public JobBillingElementCreationSource  CreationSource          { get; } 
}

/// A job billing trip charge line item
public record JobBillingTripChargeLineItem(
    NonEmptyText                        Description,
    DateTimeOffset                      RequestedByDate,
    DateTimeOffset                      ArrivalDate,
    RateType                            RateType,
    TripChargeRate                      TripChargeRate,
    LineItemCost                        LineItemCost,
    bool                                IsTripPayable,
    bool                                IsRequestedByDateMissed, 
    JobBillingElementCreationSource     CreationSource) : IJobBillingTripChargeLineItem;

/// Job flat rate section on job billing
public record JobBillingJobFlatRate(
    decimal                                         AdjustedTotalCost,
    // required collections
    Lst<JobBillingJobFlatRateLineItem>              LineItems,
    Lst<IJobBillingCostDiscountLineItem>            CostDiscounts,
    Lst<JobBillingRuleMessage>                      RuleMessages);

/// Line item for job flat rate section on job billing
public record JobBillingJobFlatRateLineItem(
    LineItemId                                      Id,
    decimal                                         Quantity,
    decimal                                         Rate,
    decimal                                         LineItemCost,
    bool                                            IsItemPayable,
    bool                                            IsFlaggedBySystem,
    bool                                            IsManuallyVerified,
    JobBillingElementCreationSource                 CreationSource,
    // optionals
    Option<NonEmptyText>                            Reason,
    // required collections
    Lst<JobBillingRuleMessage>                      RuleMessages);

/// Material/part flat rate section on job billing
public record JobBillingMaterialPartFlatRate(
    decimal                                         AdjustedTotalCost,
    // collections
    Lst<JobBillingMaterialPartFlatRateLineItem>     LineItems,
    Lst<IJobBillingCostDiscountLineItem>            CostDiscounts,
    Lst<JobBillingRuleMessage>                      RuleMessages);

/// Line item for material/part flat rate section on job billing
public record JobBillingMaterialPartFlatRateLineItem(
    LineItemId                                      Id,
    decimal                                         Quantity,
    decimal                                         Rate,
    decimal                                         LineItemCost,
    CatalogItemId                                   CatalogItemId,
    MaterialPartCatalogItemType                     CatalogItemType,
    bool                                            IsItemPayable,
    bool                                            IsFlaggedBySystem,
    bool                                            IsManuallyVerified,
    JobBillingElementCreationSource                 CreationSource,
    // optionals
    Option<NonEmptyText>                            Reason,
    // required sections
    RecordMeta                                      RecordMeta,
    // required collections
    Lst<JobBillingRuleMessage>                      RuleMessages);

/// Equipment flat rate section on job billing
public record JobBillingEquipmentFlatRate(
    decimal                                         AdjustedTotalCost,
    // required collections
    Lst<JobBillingEquipmentFlatRateLineItem>        LineItems,
    Lst<IJobBillingCostDiscountLineItem>            CostDiscounts,
    Lst<JobBillingRuleMessage>                      RuleMessages);

/// Line item for material/part flat rate section on job billing
public record JobBillingEquipmentFlatRateLineItem(
    LineItemId                                      Id,
    decimal                                         Quantity,
    decimal                                         Rate,
    decimal                                         LineItemCost,
    CatalogItemId                                   CatalogItemId,
    EquipmentCatalogItemType                        CatalogItemType,
    bool                                            IsItemPayable,
    bool                                            IsFlaggedBySystem,
    bool                                            IsManuallyVerified,
    JobBillingElementCreationSource                 CreationSource,
    // optionals
    Option<NonEmptyText>                            Reason,
    // required sections
    RecordMeta                                      RecordMeta,
    // required collections
    Lst<JobBillingRuleMessage>                      RuleMessages);

public record JobBillingProcessingFee(
    Lst<JobBillingProcessingFeeLineItem>       LineItems);

public record JobBillingProcessingFeeLineItem(
    NonEmptyText                    Description,
    ProcessingFee                   ProcessingFee);

/// Interface for cost discount on job billing 
public interface IJobBillingCostDiscountLineItem
{
    public decimal                          CostDiscount   { get; }
    public RecordMeta                       RecordMeta     { get; }
    public JobBillingElementCreationSource  CreationSource { get; }
}

/// Cost discount on job billing 
public record JobBillingCostDiscountLineItem( 
    decimal                             CostDiscount,
    JobBillingElementCreationSource     CreationSource,
    RecordMeta                          RecordMeta) : IJobBillingCostDiscountLineItem;

// Payment section on job billing
public record JobBillingPayment(
    PaymentAmount                           TotalAmountPaid,
    // optional scalars
    Option<NonEmptyText>                    PaymentTerms,
    // collections
    Lst<JobBillingPaymentTransaction>       Payments)
{
    // Computed fields
    public bool IsPaidByCreditCard => JobBillingRule.IsPaidByCreditCard(this);
}

// Payment on job billing
public record JobBillingPaymentTransaction(
    PaymentAmount                   AmountPaid,
    IJobBillingPaymentMethod        PaymentMethod);

/// A payment method on a job billing
public interface IJobBillingPaymentMethod {}

// Credit card payment method on job billing
public record JobBillingPaymentMethodCreditCard(
    CreditCardProvider              CreditCardProvider,
    DateTimeOffset                  PaidAtDateTime,
    // optional scalars
    Option<NonEmptyText>            Last4Digits,
    Option<NonEmptyText>            TransactionReferenceCode) : IJobBillingPaymentMethod;

// Electronic Fund Transfer payment method on job billing
public record JobBillingPaymentMethodElectronicFundTransfer(
    DateTimeOffset                  PaidAtDateTime,
    // optional scalars
    Option<NonEmptyText>            TransactionReferenceCode) : IJobBillingPaymentMethod;