using LanguageExt;
using static LanguageExt.Prelude;
using DMG.ProviderInvoicing.BL.Utility;

namespace DMG.ProviderInvoicing.DT.Domain;

// The purpose of this file is to model the domain specification related to mutating
// entities in the provider invoicing system.
//
// It should be free of any implementation. There should be no direct dependencies
// on database model, database category (SQL vs NoSQL), UI, APIs or external I/O sources.

/// Meta data required by any patch of a job billing
public record JobBillingPatchMeta(
    UserId                          SessionUserId,
    JobWorkId                       JobWorkId,
    ProviderOrgId                   ProviderOrgId,
    TaskId                          TaskId);

////////////////////////////////////////////////////////////////////////
/// Validated patch of the job billing 
public record JobBillingPatch(
    JobBillingId                                JobBillingId,
    JobBillingVersion                           Version,
    JobBillingPatchMeta                         Meta,
    // optionals
    Option<NonEmptyText>                        JobSummary,
    Option<NonEmptyText>                        ProviderInvoiceNumber,
    // detail collections
    Lst<JobBillingPatchMaterialPartLineItem>    MaterialPartItems,
    Lst<JobBillingPatchEquipmentLineItem>       EquipmentItems);
/// Validated job billing patch item that is material/part or equipment
public interface IJobBillingPatchMaterialPartEquipmentItem { }
/// Validated job billing patch item that is material/part or equipment and in the catalog
public record JobBillingPatchMaterialPartEquipmentCatalogItem(
    CatalogItemId                               CatalogItemId,
    Lst<JobBillingPatchCatalogItemRuleValue>    RuleValues): IJobBillingPatchMaterialPartEquipmentItem;
/// Validated job billing patch item that is material/part or equipment but not in the catalog
public record JobBillingPatchMaterialPartEquipmentNonCatalogItem(
    NonEmptyText                                Name, 
    MaterialPartEquipmentCatalogItemType        CatalogItemType) : IJobBillingPatchMaterialPartEquipmentItem;
/// Validated job billing patch material/part line item
public record JobBillingPatchMaterialPartLineItem(
    Guid                                            Id,
    IJobBillingPatchMaterialPartEquipmentItem       Item,
    decimal                                         Quantity,
    UnitOfMeasure                                   UnitType,
    decimal                                         ItemCost);
/// Validated job billing patch equipment line item
public record JobBillingPatchEquipmentLineItem(
    Guid                                            Id,
    IJobBillingPatchMaterialPartEquipmentItem       Item,
    decimal                                         Quantity,
    UnitOfMeasure                                   UnitType,
    decimal                                         ItemCost);
public record JobBillingPatchCatalogItemRuleValue(
    NonEmptyText                                    RuleName,
    NonEmptyText                                    RuleValue);

////////////////////////////////////////////////////////////////////////
/// A job billing patch success result. Accounts for case of partial success
/// where the submit for invoicing failed.
public record JobBillingPatchSuccess(
    // Job billing data returned from SOR for successful patch
    JobBillingGross                                 JobBilling);

////////////////////////////////////////////////////////////////////////
/// Unvalidated patch of the job billing 
public record JobBillingPatchUnvalidated(
    JobBillingId                                            JobBillingId,
    JobBillingVersion                                       Version,
    JobBillingPatchMeta                                     Meta,
    // optionals
    string?                                                 JobSummary,
    string?                                                 ProviderInvoiceNumber,   
    // detail collections
    Lst<JobBillingPatchUnvalidatedMaterialPartLineItem>     MaterialPartItems,
    Lst<JobBillingPatchUnvalidatedEquipmentLineItem>        EquipmentItems);
/// Unvalidated job billing patch item that is material/part or equipment
public interface IJobBillingPatchUnvalidatedMaterialPartEquipmentItem { }
/// Unvalidated job billing patch item that is material/part or equipment and in the catalog
public record JobBillingPatchUnvalidatedMaterialPartEquipmentCatalogItem(
    CatalogItemId                                           CatalogItemId,
    Lst<JobBillingPatchUnvalidatedCatalogItemRuleValue>     RuleValues) : IJobBillingPatchUnvalidatedMaterialPartEquipmentItem;
/// Unvalidated job billing patch item that is material/part or equipment but not in the catalog
public record JobBillingPatchUnvalidatedMaterialPartEquipmentNonCatalogItem(
    string                                      Name, 
    MaterialPartEquipmentCatalogItemType        CatalogItemType) : IJobBillingPatchUnvalidatedMaterialPartEquipmentItem;
/// Unvalidated job billing patch material/part line item
public record JobBillingPatchUnvalidatedMaterialPartLineItem(
    // required
    IJobBillingPatchUnvalidatedMaterialPartEquipmentItem        Item,
    decimal                                                     Quantity,
    UnitOfMeasure                                               UnitType,
    decimal                                                     ItemCost,
    // optionals
    Guid?                                                       Id); // optional to allow insert
/// Unvalidated job billing patch equipment line item
public record JobBillingPatchUnvalidatedEquipmentLineItem(
    // required
    IJobBillingPatchUnvalidatedMaterialPartEquipmentItem        Item,
    decimal                                                     Quantity,
    UnitOfMeasure                                               UnitType,
    decimal                                                     ItemCost,
    // optionals
    Guid?                                                       Id); // optional to allow insert
public record JobBillingPatchUnvalidatedCatalogItemRuleValue(
    string                                      RuleName,
    string                                      RuleValue);

////////////////////////////////////////////////////////////////////////
/// Validated patch of a job billing labor line item technician trip
public record JobBillingLaborLineItemTechnicianTripPatch(
    JobBillingId                JobBillingId,
    JobBillingVersion           Version,
    JobBillingPatchMeta         Meta,
    TechnicianTripId            TechnicianTripId,
    DateTimeOffset              CheckInDateTime,
    DateTimeOffset              CheckOutDateTime);

////////////////////////////////////////////////////////////////////////
/// A job billing labor line item technician trip patch success result. 
public record JobBillingLaborLineItemTechnicianTripPatchSuccess(
    // Job billing data returned from SOR for successful patch
    JobBillingGross             JobBilling
);

////////////////////////////////////////////////////////////////////////
/// Unvalidated patch of a job billing labor line item technician trip
public record JobBillingLaborLineItemTechnicianTripPatchUnvalidated(
    Guid                        JobBillingId,
    int                         Version,
    JobBillingPatchMeta         Meta,
    Guid                        TechnicianTripId,
    DateTimeOffset              CheckInDateTime,
    DateTimeOffset              CheckOutDateTime);

/// Meta data required by job billing submit for invoicing
public record JobBillingSubmitForInvoicingMeta(
    UserId                          SessionUserId,
    JobWorkId                       JobWorkId,
    ProviderOrgId                   ProviderOrgId,
    TaskId                          TaskId);

/// Validated submit-for-invoicing of the job billing 
public record JobBillingSubmitForInvoicing(
    JobBillingId?                                JobBillingId,
    ProviderBillingId?                           ProviderBillingId,
    JobBillingSubmitForInvoicingMeta            Meta);

/// Unvalidated submit-for-invoicing of the job billing 
public record JobBillingSubmitForInvoicingUnvalidated(
    JobBillingId?                                JobBillingId,
    ProviderBillingId?                           ProviderBillingId,
    JobBillingSubmitForInvoicingMeta            Meta);

/// Provides the result of submitting a job billing for invoicing. 
public record JobBillingSubmitForInvoicingResult(
    JobBillingId?                        JobBillingId,
    ProviderBillingId?                   ProviderBillingId
    // TODO other attributes expected like JobWorkState
);

/// Meta data required by job billing review request create
public record JobBillingReviewRequestCreateMeta(
    UserId                              SessionUserId,
    JobWorkId                           JobWorkId,
    JobBillingId                        JobBillingId,
    ProviderOrgId                       ProviderOrgId);
    
/// Validated job billing review request create
public record JobBillingReviewRequestCreate(
    NonEmptyText                            Message,
    JobBillingReviewRequestCreateMeta       Meta);
    
/// Unvalidated job billing review request create
public record JobBillingReviewRequestCreateUnvalidated(
    string                                  Message,
    JobBillingReviewRequestCreateMeta       Meta);

/// Meta data required by job billing dispute response put
public record JobBillingDisputeResponsePutMeta(
    UserId                                  SessionUserId,
    JobWorkId                               JobWorkId,
    TaskId                                  TaskId,
    ProviderOrgId                           ProviderOrgId);

/// Valid line item types for a job billing dispute response
public enum JobBillingDisputeResponsePutLineItemType 
{
    MaterialPart,
    Equipment,
    Labor,
    FlatRate
}

/// Validated job billing dispute response put
public record JobBillingDisputeResponsePut(
    JobBillingId                                    JobBillingId,
    NonEmptyText                                    DisputeResponseMessage,
    LineItemId                                      LineItemId,
    JobBillingDisputeResponsePutLineItemType        LineItemType,
    JobBillingDisputeResponsePutMeta                Meta);
    
/// Unvalidated job billing dispute response put
public record JobBillingDisputeResponsePutUnvalidated(
    Guid                                            JobBillingId,
    string?                                         DisputeResponseMessage,
    Guid                                            LineItemId,
    JobBillingDisputeResponsePutLineItemType        LineItemType,
    JobBillingDisputeResponsePutMeta                Meta);    
    
#region JobPhoto
/// Meta data required by all job photo mutations
public record JobPhotoMutationMeta(
    UserId                          SessionUserId,
    ProviderOrgId                   ProviderOrgId,
    TaskId                          TaskId);

/// Job photo to be put
public record JobPhotoPut(
    JobPhotoMutationMeta            Meta,
    JobPhotoBase                    Base );

/// Job photos to be deleted
public record JobPhotosDelete(
    JobPhotoMutationMeta            Meta,
    JobWorkId                       JobWorkId,
    Lst<JobPhotoId>                 JobPhotoIds );

/// Unvalidated job photo to be put
public record JobPhotoPutUnvalidated(
    JobPhotoMutationMeta            Meta,
    JobPhotoPutBaseUnvalidated      Base );

/// Unvalidated job photo base to be put
public record JobPhotoPutBaseUnvalidated(
    Guid                            JobWorkId,
    // Holds both the PK and ID of the file in the large object store
    Guid                            JobPhotoId,
    Guid?                            ServiceLineItemId,
    Guid?                            VisitId,
    string                          MimeType,
    PhotoChronology                 PhotoChronology,
    string?                         FileName,
    string?                         Description);
#endregion JobPhoto

#region JobDocument
public record JobDocumentPut(
    JobDocumentMutationMeta         Meta,
    JobDocumentBase                 Base );

public record JobDocumentMutationMeta(
    UserId                          SessionUserId,
    ProviderOrgId                   ProviderOrgId,
    TaskId                          TaskId);

// The request for deleting job documents.
public record JobDocumentsDelete(
    JobDocumentMutationMeta         Meta,
    JobWorkId                       JobWorkId,
    Lst<JobDocumentId>              JobDocumentIds);    

public record JobDocumentPutBaseUnvalidated(
    Guid                            JobWorkId,
    // Holds both the PK and ID of the file in the large object store
    Guid                            JobDocumentId,
    string                          MimeType,
    string?                         FileName,
    string?                         Description );
public record JobDocumentPutUnvalidated(
    JobDocumentMutationMeta         Meta,
    JobDocumentPutBaseUnvalidated   Base );
#endregion JobDocument