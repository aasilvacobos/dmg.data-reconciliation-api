using LanguageExt;
using static LanguageExt.Prelude;
using DMG.ProviderInvoicing.BL.Utility;

namespace DMG.ProviderInvoicing.DT.Domain;

// The purpose of this file is to model the provider invoicing domain in regard to
// entities that have been persisted in the local database. This type of entity
// uses composition to provide additional fields that are not part of the core entity.
// They should be used as the return for methods in the database API.

/// Status of a job billing's conversion into a provide invoice transaction 
public enum JobBillingInvoicingStatus
{
    Pending,
    Success,
    Failure,
    Bypassed,
    Voided,
    Undefined
}

/// Job billing that exists in the FAL. Use only for retrieval, not insert/update.
public record JobBillingFalDatabasePersisted(
    JobBilling                          Core,
    // Meta fields unique to local database
    // required
    JobBillingInvoicingStatus           InvoicingStatus,
    DateTimeOffset                      CreatedOnDateTime,
    DateTimeOffset                      ModifiedOnDateTime,
    // optionals
    Option<NonEmptyText>                DmgInvoiceNumber);