using DMG.ProviderInvoicing.BL.Utility;
using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain;

// The purpose of this file is to model the domain specification related to mutating
// entities external to provider invoicing
//
// It should be free of any implementation. There should be no direct dependencies
// on database model, database category (SQL vs NoSQL), UI, APIs or external I/O sources.

/// Types of items accepted by FAL/NetSuite
public enum FalItemType 
{ 
    MaterialPart,
    Equipment,
    Labor,
    Trip,
    ProcessingFee,
    FlatRateJob,
    RoutineLabor
}

/// A single line on the vendor bill that is sent to FAL/NetSuite for insert
public record VendorBillLineInsert(
    ProviderInvoiceId               ExternalId,
    NonEmptyText                    TicketNumber,
    TicketId                        TicketId,
    ProviderOrgId                   VendorReference,
    DateTimeOffset                  TransactionDate,
    DateTimeOffset                  PaymentDueDate,
    NonEmptyText                    JobSummary,
    PropertyId                      PurchaseItemLineCustomerRef,
    NonEmptyText                    PurchaseItemLineClassReference, // service line name
    NonEmptyText                    JobWorkNumber,
    JobWorkId                       JobWorkId,
    JobBillingId                    JobBillingId,
    NonEmptyText                    DmgInvoiceNumber,               
    NonEmptyText                    ContractType,
    NonEmptyText                    Currency,
    NonEmptyText                    DmgProviderInvoiceStatus,
    NonEmptyText                    BillStatus,
    uint                            PaymentTermsReference,          // TODO rename to PaymentTermsIdentifier
    bool                            IsCreditCardPayment,
    bool                            IsHold,
    bool                            IsPrepaid,    
    // line-level attributes
    FalItemType                     PurchaseItemLineItemRef,
    decimal                         PurchaseItemLineQuantity,
    decimal                         PurchaseItemLineRate,
    decimal                         PurchaseItemLineAmount,
    NonEmptyText                    PurchaseItemLineLocationRef,    // servicing address state name
    // optionals
    Option<NonEmptyText>            HoldReason,
    Option<DateTimeOffset>          HoldChangeDate,
    Option<NonEmptyText>            PaymentMethod,
    Option<NonEmptyText>            ProviderInvoiceNumber,
    Option<NonEmptyText>            PurchaseItemLineDescription,
    Option<NonEmptyText>            PurchaseItemLineReference,
    Option<NonEmptyText>            RateTypeText,
    Option<decimal>                 LaborQuantityInMinutes,
    Option<NonEmptyText>            Memo);
    
/// Represents a result of successfully inserting all lines for a vendor bill 
public abstract record VendorBillInsertSuccess(
    JobBilling                      JobBilling, 
    NonEmptyText                    DmgInvoiceNumber,
    int                             VendorBillLineCount, 
    JobBillingCostingScheme         CostingScheme)  
{
    private record VendorBillInsertSuccessConcrete(JobBilling JobBilling, NonEmptyText DmgInvoiceNumber, int VendorBillLineCount, JobBillingCostingScheme CostingScheme) 
        : VendorBillInsertSuccess(JobBilling, DmgInvoiceNumber, VendorBillLineCount, CostingScheme);

    public static VendorBillInsertSuccess Create(Lst<VendorBillLineInsert> vendorBillLineInserts, JobBilling jobBilling) =>
        new VendorBillInsertSuccessConcrete(
            jobBilling,
            vendorBillLineInserts.Count > 0 ? vendorBillLineInserts[0].DmgInvoiceNumber : NonEmptyText.NewUnsafe("Undefined"),
            vendorBillLineInserts.Count, 
            jobBilling.CostingScheme);
}    