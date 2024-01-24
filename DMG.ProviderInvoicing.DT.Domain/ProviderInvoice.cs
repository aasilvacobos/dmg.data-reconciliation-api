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

/// Fields share by provider invoice entities
public interface IProviderInvoice 
{
    ProviderInvoiceId                           ProviderInvoiceId       { get; }
    JobBillingId                                JobBillingId            { get; }  
    ProviderOrgId                               ProviderOrgId           { get; }
    // TODO BillingContractType                         ContractType            { get; }
    // TODO CostingScheme                               CostingScheme            { get; }
    DateTimeOffset                              TransactionDate         { get; }
    NonEmptyText                                ServiceLineName         { get; }        
    NonEmptyText                                ServiceStateName        { get; }
    // optional scalars
    Option<NonEmptyText>                        ProviderInvoiceNumber   { get; }
    // detail collections
}

///////////////////////////////////////////////////////
/// Transactional Data **AGGREGATE**: Provider Invoice
/// A provider invoice that is to be inserted (created) into the SOR data store.
/// Also used to map to list of VendorBillLineInserts. 
public record ProviderInvoiceInsert(
    ProviderInvoiceId                               ProviderInvoiceId,
    // TODO BillingContractType                             ContractType,
    JobBillingId                                    JobBillingId,
    ProviderOrgId                                   ProviderOrgId,
    JobWorkId                                       JobWorkId,          // TODO move to new job association section
    TicketId                                        TicketId,
    CustomerId                                      CustomerId,
    PropertyId                                      PropertyId,
    ServiceLineId                                   ServiceLineId,
    NonEmptyText                                    ServiceLineName,
    ServiceTypeId                                   ServiceTypeId,      // TODO move to new job association section
    NonEmptyText                                    JobSummary,
    NonEmptyText                                    TicketNumber,
    NonEmptyText                                    JobWorkNumber,      // TODO move to new job association section
    NonEmptyText                                    ServiceStateName,   // TODO rename as ServicingAddressStateName
    DateTimeOffset                                  TransactionDate,
    JobBillingCostingScheme                         CostingScheme,
    // TODO NonEmptyText                            ProviderInvoiceStatus
    RecordMeta                                      RecordMeta,
    NonEmptyText                                    DmgInvoiceNumber, 
    // optionals
    Option<NonEmptyText>                            ProviderInvoiceNumber,
    // detail collection
    Lst<ProviderInvoiceMaterialPartLineItem>        MaterialPartLineItems,
    Lst<ProviderInvoiceEquipmentLineItem>           EquipmentLineItems,
    Lst<ProviderInvoiceLaborLineItem>               LaborLineItems,
    Lst<ProviderInvoiceTripChargeLineItem>          TripChargeLineItems,
    Lst<ProviderInvoiceJobFlatRateLineItem>         JobFlatRateLineItems,
    Lst<ProviderInvoiceProcessingFeeLineItem>       ProcessingFeeLineItems,
    Lst<ProviderInvoiceCostDiscountLineItem>        MaterialCostDiscountLineItems,
    Lst<ProviderInvoiceCostDiscountLineItem>        EquipmentCostDiscountLineItems,
    Lst<ProviderInvoiceCostDiscountLineItem>        LaborCostDiscountLineItems,
    Lst<ProviderInvoiceCostDiscountLineItem>        JobFlatRateCostDiscountLineItems,
    // sections
    ProviderInvoicePayment                          Payment) : IProviderInvoice;

///////////////////////////////////////////////////////
/// Transactional Data **AGGREGATE**: Provider Invoice
/// A provider invoice that is already persisted 
public record ProviderInvoice(
    ProviderInvoiceId                               ProviderInvoiceId,
    BillingContractType                             ContractType,
    JobBillingId                                    JobBillingId,
    ProviderOrgId                                   ProviderOrgId,
    JobWorkId                                       JobWorkId,
    TicketId                                        TicketId,
    CustomerId                                      CustomerId,
    PropertyId                                      PropertyId,
    ServiceLineId                                   ServiceLineId,
    NonEmptyText                                    ServiceLineName,
    ServiceTypeId                                   ServiceTypeId,
    NonEmptyText                                    JobSummary,
    NonEmptyText                                    TicketNumber,
    NonEmptyText                                    JobWorkNumber,
    NonEmptyText                                    ServiceStateName,   // TODO rename as ServicingAddressStateName
    DateTimeOffset                                  TransactionDate,
    JobBillingCostingScheme                         CostingScheme,
    NonEmptyText                                    ProviderInvoiceStatus,
    NonEmptyText                                    DmgInvoiceNumber, 
    decimal                                         TotalCost,
    // required sections
    ProviderInvoiceVendorBill                       VendorBill,
    // optionals
    Option<NonEmptyText>                            ProviderInvoiceNumber,
    // detail collection
    Lst<ProviderInvoiceMaterialPartLineItem>        MaterialPartLineItems,
    Lst<ProviderInvoiceEquipmentLineItem>           EquipmentLineItems,
    Lst<ProviderInvoiceLaborLineItem>               LaborLineItems,
    Lst<ProviderInvoiceTripChargeLineItem>          TripChargeLineItems,
    Lst<ProviderInvoiceJobFlatRateLineItem>         JobFlatRateLineItems,
    Lst<ProviderInvoiceProcessingFeeLineItem>       ProcessingFeeLineItems,
    Lst<ProviderInvoiceCostDiscountLineItem>        MaterialCostDiscountLineItems,
    Lst<ProviderInvoiceCostDiscountLineItem>        EquipmentCostDiscountLineItems,
    Lst<ProviderInvoiceCostDiscountLineItem>        LaborCostDiscountLineItems,
    Lst<ProviderInvoiceCostDiscountLineItem>        JobFlatRateCostDiscountLineItems) : IProviderInvoice;

/// A provider invoice item that is material/part or equipment
public interface IProviderInvoiceMaterialPartEquipmentItem 
{
    NonEmptyText Name { get; }
}

/// A provider invoice item that is material/part or equipment and in the catalog
public record ProviderInvoiceMaterialPartEquipmentCatalogItem(
    CatalogItemId               CatalogItemId,  // TODO should be able to remove
    NonEmptyText                Name) : IProviderInvoiceMaterialPartEquipmentItem;

/// A provider invoice item that is material/part or equipment but not in the catalog
public record ProviderInvoiceMaterialPartEquipmentNonCatalogItem(
    NonEmptyText                Name) : IProviderInvoiceMaterialPartEquipmentItem;

/// A provider invoice material/part line item
public record ProviderInvoiceMaterialPartLineItem(
    Guid                                        Id,
    MaterialPartCatalogItemType                 CatalogItemType,
    IProviderInvoiceMaterialPartEquipmentItem   Item,
    decimal                                     Quantity,
    ItemCost                                    ItemCost, 
    LineItemCost                                LineItemCost,
    NonEmptyText                                LineReference);         // this is basically the part number that goes into netsuite);

/// A provider invoice equipment line item
public record ProviderInvoiceEquipmentLineItem(
    Guid                                        Id,
    EquipmentCatalogItemType                    CatalogItemType,
    IProviderInvoiceMaterialPartEquipmentItem   Item,
    decimal                                     Quantity,
    ItemCost                                    ItemCost, 
    LineItemCost                                LineItemCost,
    NonEmptyText                                LineReference);         // this is basically the part number that goes into netsuite

/// A provider invoice labor item
public record ProviderInvoiceLaborLineItem(
    TechnicianType                              TechnicianType,
    decimal                                     Hours,
    RateType                                    RateType,
    LaborRate                                   LaborRate,
    LineItemCost                                LineItemCost,
    NonEmptyText                                Description,
    NonEmptyText                                LineReference);         // this is basically the part number that goes into netsuite

// fees charged to the provider
public record ProviderInvoiceProcessingFeeLineItem(
    decimal                                     Quantity,
    ProcessingFee                               ProcessingFee,
    LineProcessingFee                           LineProcessingFee,
    NonEmptyText                                Description,
    NonEmptyText                                LineReference);         // this is basically the part number that goes into netsuite

/// A provider invoice trip charge line item
public record ProviderInvoiceTripChargeLineItem(
    NonEmptyText                    Description,
    DateTimeOffset                  RequestedByDate,
    DateTimeOffset                  ArrivalDate,
    RateType                        RateType,
    TripChargeRate                  TripChargeRate,
    LineItemCost                    LineItemCost,
    bool                            IsRequestedByDateMissed,
    NonEmptyText                    LineReference);

/// A provider invoice item that is flat rate
public interface IProviderInvoiceFlatRateItem { }

/// A provider invoice item that is flat rate catalog
public record ProviderInvoiceFlatRateCatalogItem(
    CatalogItemId                           CatalogItemId,
    MaterialPartEquipmentCatalogItemType    CatalogItemType,
    NonEmptyText                            Name) : IProviderInvoiceFlatRateItem;

/// A provider invoice item that is flat rate job
public record ProviderInvoiceFlatRateJobItem(
    NonEmptyText                            Name): IProviderInvoiceFlatRateItem;

/// A provider invoice job flat rate line item
public record ProviderInvoiceJobFlatRateLineItem(
    LineItemId                                      Id,
    decimal                                         Quantity,
    decimal                                         Rate,
    decimal                                         LineItemCost,
    bool                                            IsItemPayable,
    bool                                            IsFlaggedBySystem,
    bool                                            IsManuallyVerified,
    NonEmptyText                                    ServiceTypeName,
    JobBillingElementCreationSource                 CreationSource,
    NonEmptyText                                    LineReference,
    // optionals
    Option<NonEmptyText>                            Reason);

// Payment section on job billing
public record ProviderInvoicePayment(
    PaymentAmount                           TotalAmountPaid,
    bool                                    IsPaidByCreditCard, // passed through from job billing value which was derived from on rule
    // optional scalars
    Option<NonEmptyText>                    PaymentTerms
    // collections
    //Lst<ProviderInvoicePaymentTransaction>       Payments    // TODO
    );

/// A provider invoice cost discount line item
public record ProviderInvoiceCostDiscountLineItem(
    decimal                         CostDiscount,
    NonEmptyText                    LineReference,
    NonEmptyText                    Description);
    
/// Supplemental data required to build a provider invoice 
public record ProviderInvoiceBuilderSupplement(
    Lst<CatalogItem>                CatalogItems,
    NonEmptyText                    DmgInvoiceNumber,
    NonEmptyText                    ServiceLineName,
    // optionals
    Option<NonEmptyText>            ServiceTypeName,    // only need for flat rate costing
    Option<NonEmptyText>            ServicingAddressStateName);

/// Data from vendor bill that is associated to a provider invoice 
public record ProviderInvoiceVendorBill(
    NonEmptyText                    BillStatus,
    PaymentTermsIdentifier          PaymentTermsIdentifier,
    DateTimeOffset                  PaymentDueDate,
    bool                            IsPrepaid)
{
    // Computed fields
    public NonEmptyText PaymentTermsText => PaymentTermsRule.ToText(this.PaymentTermsIdentifier);
}  