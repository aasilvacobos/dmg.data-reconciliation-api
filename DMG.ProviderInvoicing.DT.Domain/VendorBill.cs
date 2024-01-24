using DMG.ProviderInvoicing.DT.Domain.Rule;
using DMG.ProviderInvoicing.BL.Utility;
using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain;

/// Represents a persisted vendor bill (in the FAL & NetSuite). Should be used for a retrieve/get operations.
/// Add additional fields and line items only as necessary
public record VendorBill(
    ProviderInvoiceId               ExternalId,
    JobBillingId                    JobBillingId,  // TODO will be ProviderBillingId in future
    Option<String>                  TransactionType,                
    ProviderOrgId                   ProviderOrgId,
    NonEmptyText                    DmgInvoiceNumber,               
    DateTimeOffset                  TransactionDate,
    DateTimeOffset                  PaymentDueDate,
    BillingContractType             ContractType,
    NonEmptyText                    ProviderInvoiceStatus,
    NonEmptyText                    BillStatus,
    NonEmptyText                    PurchaseItemLineClassReference, // service line name
    NonEmptyText                    PurchaseItemLineLocationRef,    // servicing address state name
    PaymentTermsIdentifier          PaymentTermsIdentifier,
    bool                            IsPrepaid,
    // optional scalars
    Option<NonEmptyText>            ProviderInvoiceNumber);