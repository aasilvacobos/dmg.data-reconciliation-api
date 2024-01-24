using DMG.ProviderInvoicing.BL.Utility;
using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.DT.Domain;

public record FeatureFlag(
    FeatureFlagId               FeatureFlagId,
    NonEmptyText                Code,
    NonEmptyText                Description,
    bool                        IsActive,
    DateTimeOffset              CreatedOn,
    DateTimeOffset              ModifiedOn);

public record  FeatureFlagSearchInput(
    // optionals
    Option<ProviderOrgId>   ProviderOrgIdOption,
    Option<UserId>          UserIdOption);