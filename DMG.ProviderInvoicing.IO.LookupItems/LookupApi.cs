using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Domain;
using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.IO.LookupItems;

// Specifies the lookup type. Enum integer value used in I/O request.
public enum LookupDataSetType
{
    Index               = 0,
    State               = 1,
    // CatalogDataSource = 2,
    // PropertyType      = 3,
    // PropertySpotType  = 4,
    // PostalCode        = 5,
    // ServiceLineAsset  = 6
    PaymentTerm         = 7
}

/// <summary>
/// API consumed by other I/O adapters to retrieve from the Lookup Items system.
/// </summary>
public static class LookupApi
{
    public static bool Ping() => 
        LookupClient.Ping();

    /// Retrieve lookup item data set by type
    public static Option<LookupDataSetCore> TryGetLookupDataSetCore(LookupDataSetType lookupDataSetType) =>
        LookupClient.TryGetLookupDataSetCore(lookupDataSetType);
    
    /// Retrieve index of all lookup item data sets
    public static Option<LookupDataSetCore> TryGetLookupDataSetCoreIndex() =>
        LookupClient.TryGetLookupDataSetCoreIndex();

    /// Return the value for a name (i.e., key) for an given lookup item data set
    public static Option<NonEmptyText> TryFindValueByName(LookupDataSetType lookupDataSetType, NonEmptyText name) =>
        LookupClient.TryFindValueByName(lookupDataSetType, name);
}