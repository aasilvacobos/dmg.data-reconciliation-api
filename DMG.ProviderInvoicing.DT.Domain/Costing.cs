using DMG.ProviderInvoicing.DT.Domain.Rule;
using LanguageExt;
using static LanguageExt.Prelude;
using DMG.ProviderInvoicing.BL.Utility;

namespace DMG.ProviderInvoicing.DT.Domain;


public enum CostingSchemeFlatRateItemType
{
    Job,
    CatalogItem
}

/// A job costing scheme that is time & materials or flat rate (DU)
public interface ICostingScheme
{
    public ProviderNotToExceedAmount ProviderNotToExceedAmount { get; init; }
}
/// Time & material costing scheme 
public record TimeAndMaterialCostingScheme(
    bool                                IsLatest,
    ProviderNotToExceedAmount           ProviderNotToExceedAmount,
    LaborRate                           LaborRegularRate,         
    LaborRate                           LaborHelperRate,       
    TripChargeRate                      TripChargeRate) : ICostingScheme;
/// Flat rate costing scheme 
public record FlatRateCostingScheme(
    ProviderNotToExceedAmount           ProviderNotToExceedAmount,
    // collections
    Lst<FlatRateCostingSchemeItem>      Items) : ICostingScheme;

/// Item in flat rate costing scheme
public record FlatRateCostingSchemeItem(
    FlatRateCostingSchemeItemId         Id,
    CostingSchemeFlatRateItemType       ItemType,
    decimal                             Rate,
    // optionals
    Option<CatalogItemId>               CatalogItemId);

/// Costing data stored in job
public record Costing(
    JobCostingId                        JobCostingId,
    RateType                            RateType,
    ICostingScheme                      CostingScheme);