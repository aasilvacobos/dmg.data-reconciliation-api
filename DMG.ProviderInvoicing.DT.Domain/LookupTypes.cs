using DMG.ProviderInvoicing.BL.Utility;
using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain;

/// Master Data **AGGREGATE**: Lookup Core in DMG Pro system
public record LookupDataSetCore(
    LookupDataSetId             DataSetId,
    Option<NonEmptyText>        DataSetName,
    // collections
    Lst<LookupItemCore>         Items);
public record LookupItemCore(
    NonEmptyText                Name,   // i.e, key/code
    NonEmptyText                Value);

/// Master Data **AGGREGATE**: Lookup in DMG Pro system
public record LookupDataSet(
    LookupDataSetId             DataSetId,
    Option<NonEmptyText>        DataSetName,
    // collections
    Lst<LookupItem>             Items);
public record LookupItem(
    LookupItemId                LookupItemId,
    LookupDataSetId             DataSetId,
    NonEmptyText                Name,   // i.e, key/code
    NonEmptyText                Value,
    // optionals
    Option<NonEmptyText>        Description,
    Option<LookupItemId>        ParentLookupItemId);