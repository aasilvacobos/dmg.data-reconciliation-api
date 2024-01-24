using DMG.ProviderInvoicing.BL.Utility;
using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain;

// The purpose of this file is to model the domain specification that is external to
// provider invoicing, serving as both documentation and code.
// It should be free of any implementation. There should be no direct dependencies
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

///////////////////////////////////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////////////////////////////
// Global/shared enumeration and record types within external systems

/// Types of catalog items
public enum CatalogItemType 
{
    Unspecified,
    Material,
    Part,
    EquipmentOwned,
    EquipmentRental,
    Labor,
    Trip,
    FlatRate
}

// Types of property address
public enum AddressType
{
    Unspecified,
    Billing,
    Servicing,
    BillingAndServicing
}

// Source of variance for a catalog item availability range
public enum VarianceSourcing
{
    Unspecified,
    Fixed,
    Actual,
    Internal
}

// Cost/price enforcement level for a catalog item availability range
public enum EnforcementLevel
{
    Unspecified,
    Variance98,
    Variance95,
    Variance68,
    VarianceMax,
    VarianceMedian
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Entities and choice types for which Provider Invoicing system is the
// system of *reference*.

/// Master Data **AGGREGATE**: User in DMG Pro system
/// Count Estimate: 10K, Daily Change Estimate: 1%
public record User(  
    UserId          UserId,
    string          FirstName,
    string          LastName);

/// Master Data **AGGREGATE**: Service line.
public record ServiceLine(
    ServiceLineId               ServiceLineId,
    string                      Name,       // TODO change to NonEmptyText
    // optionals
    Option<NonEmptyText>        Description);

/// Master Data **AGGREGATE**: Service type. This is a recursive structure
public record ServiceType(
    ServiceTypeId               ServiceTypeId,
    string                      Name,
    string                      Code,
    // TODO JobType JobType,
    // optionals
    Option<NonEmptyText>        Description);

/// Master Data **AGGREGATE**: Catalog item (within catalog entry) type. This is a recursive structure
public record CatalogItem(
    CatalogItemId               CatalogItemId,
    NonEmptyText                Name,
    CatalogItemType             CatalogItemType,
    UnitOfMeasure               UnitType,
    // optionals
    Option<Guid>                ParentId,       // Parent of catalog item. Used to link refinements to parent entities. Can be null if it is the top level entity
    Option<NonEmptyText>        Category,
    Option<NonEmptyText>        Description,    // "Air Compressor Labor - Normal"
    Option<NonEmptyText>        Brand,          // If specified then this is for a specific brand of this item. Otherwise, then this item will match any brand.
    Option<NonEmptyText>        CommonId,       // Unique key for this item; can be used on an invoice. Not UUID/GUID but a human readable item #.
    Option<NonEmptyText>        ManufacturersPartNum,
    Option<NonEmptyText>        Upc, 
    Option<NonEmptyText>        ImageIdFirst);  // first image URL found in imageId list 

/// Master Data **AGGREGATE**: Catalog item availability range.
public record CatalogItemAvailabilityRange(
    CatalogItemAvailabilityRangeId      CatalogItemAvailabilityRangeId, 
    CatalogItemId                       CatalogItemId,
    ServiceTypeId                       ServiceTypeId,
    GeoCoverageId                       GeoCoverageId,
    decimal                             MeanCost,
    EnforcementLevel                    LimitCostLevel,
    decimal                             MeanPrice,
    EnforcementLevel                    LimitPriceLeve,
    VarianceSourcing                    VarianceSourcing,
    // required sections
    Variance                            VarianceCost,
    Variance                            VariancePrice,
    RecordMeta                          RecordMeta);
public record Variance(
    VarianceValue       Variance68,
    VarianceValue       Variance95,
    VarianceValue       Variance98,
    VarianceValue       MinValue,
    VarianceValue       MaxValue,
    VarianceValue       MedianValue);

/// Master Data **AGGREGATE**: Provider organization entity 
public record ProviderOrg(  
    ProviderOrgId                   ProviderOrgId,
    NonEmptyText                    Name,
    // optional scalars
    Option<NonEmptyText>            Email,  
    // optional sections
    Option<ProviderOrgAddress>      AddressPayment);
public record ProviderOrgAddress(  
    ProviderOrgAddressId            ProviderOrgAddressId,
    // optional scalars
    Option<NonEmptyText>            Line1,
    Option<NonEmptyText>            Line2,
    Option<NonEmptyText>            City,
    Option<NonEmptyText>            County,
    Option<NonEmptyText>            State,
    Option<NonEmptyText>            Neighborhood,
    Option<NonEmptyText>            PostalCode);

/// Master Data **AGGREGATE**: Physical property where job is performed 
/// Count Estimate: 1,000K, Daily Change Estimate: 10%
public record Property(  
    PropertyId                      PropertyId,
    // optional sections TODO move to bottom
    Option<PropertyAddress>         AddressServicing,
    // optional scalars
    Option<GeoCoverageId>           GeoCoverageId,
    Option<DateTimeOffset>          CreatedOnDateTime,
    Option<DateTimeOffset>          ModifiedOnDateTime,
    Option<NonEmptyText>            PropertyFullName,
    Option<NonEmptyText>            PropertyBaseName,
    Option<NonEmptyText>            StoreNumber);
public record PropertyAddress(
    AddressId                   AddressId,
    bool                        IsPrimary,
    AddressType                 AddressType,
    NonEmptyText                Line1,
    NonEmptyText                City,
    // optionals
    Option<NonEmptyText>        Line2,
    Option<NonEmptyText>        Line3,
    Option<NonEmptyText>        Line4,
    Option<NonEmptyText>        StateCode,  // abbreviation
    Option<NonEmptyText>        Country,
    Option<NonEmptyText>        PostalCode);

/// Master Data **AGGREGATE**: The customer entity.
public record Customer(
    CustomerId                                  CustomerId,
    string                                      Name,   
    // optionals
    Option<NonEmptyText>                        CustomerNumber,
    Option<LogoPhotoId>                         LogoPhotoId);