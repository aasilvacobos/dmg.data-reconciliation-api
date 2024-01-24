using LanguageExt;
using static LanguageExt.Prelude;
using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Domain;
using static DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper.MessageMapperUtility;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;

/// Provides deterministic mapping between property protobuf messages and domain entities. It provides shared functionality
/// among any I/O adapter working with protobuf messages.
public static class PropertyMessageMapper 
{
    public static DT.Domain.AddressType ToEntity(DMG.Proto.Properties.AddressType addressTypeMessage) =>
        addressTypeMessage
            switch 
            {
                DMG.Proto.Properties.AddressType.Billing => DT.Domain.AddressType.Billing,
                DMG.Proto.Properties.AddressType.Servicing => DT.Domain.AddressType.Servicing,
                DMG.Proto.Properties.AddressType.BillingAndServicing => DT.Domain.AddressType.BillingAndServicing,
                DMG.Proto.Properties.AddressType.Unspecified => DT.Domain.AddressType.Unspecified,
                _ => DT.Domain.AddressType.Unspecified
            };
    
    private static DT.Domain.PropertyAddress BuildPropertyAddressEmpty(Domain.AddressType addressType) =>
        new PropertyAddress(
            new AddressId(Guid.Empty),
            false,
            addressType,
            NonEmptyText.NewUnsafe(DefaultRequiredStringValueIfMissing), 
            NonEmptyText.NewUnsafe(DefaultRequiredStringValueIfMissing),
            NonEmptyText.NewUnsafe(DefaultRequiredStringValueIfMissing),
            Option<NonEmptyText>.None, 
            Option<NonEmptyText>.None, 
            Option<NonEmptyText>.None, 
            Option<NonEmptyText>.None,
            Option<NonEmptyText>.None);
    
    public static DT.Domain.PropertyAddress ToEntity(DMG.Proto.Properties.PropertyAddress propertyAddressMessage) =>
        Optional(propertyAddressMessage.Address)
            .Match(address => 
                    new PropertyAddress(
                        new AddressId(ParseGuidStringDefaultToEmptyGuid(address.AddressId)),
                        address.IsPrimary,
                        ToEntity(propertyAddressMessage.PropertyAddressType),
                        NonEmptyText.NewUnsafe(address.Line1.DefaultIfNullOrWhiteSpace(DefaultRequiredStringValueIfMissing)),
                        NonEmptyText.NewUnsafe(address.City.DefaultIfNullOrWhiteSpace(DefaultRequiredStringValueIfMissing)), 
                        // optionals
                        NonEmptyText.NewOptionUnvalidated(address.Line2), 
                        NonEmptyText.NewOptionUnvalidated(address.Line3),
                        NonEmptyText.NewOptionUnvalidated(address.Line4),
                        NonEmptyText.NewOptionUnvalidated(address.State),   // abbreviation
                        NonEmptyText.NewOptionUnvalidated(address.Country), 
                        NonEmptyText.NewOptionUnvalidated(address.PostalCode)),
                () => BuildPropertyAddressEmpty(ToEntity(propertyAddressMessage.PropertyAddressType)));

    // determine if property address is a servicing address
    public static bool IsPropertyAddressServicing(DMG.Proto.Properties.PropertyAddress propertyAddressMessage) =>
        propertyAddressMessage.PropertyAddressType is Proto.Properties.AddressType.Servicing or Proto.Properties.AddressType.BillingAndServicing;

    public static Option<DMG.Proto.Properties.PropertyAddress> TryFindPropertyAddressServicingMessage(IEnumerable<DMG.Proto.Properties.PropertyAddress> propertyAddressMessages) =>
        propertyAddressMessages
            .Freeze()
            .Filter(IsPropertyAddressServicing)
            .ToOption();

    public static DT.Domain.Property ToEntity(DMG.Proto.Properties.Property propertyMessage, Option<DT.Domain.PropertyAddress> propertyAddressServicingOption) =>
        new(new PropertyId(ParseGuidStringDefaultToEmptyGuid(propertyMessage.PropertyId)),
            propertyAddressServicingOption,
            // optional scalars
            ParseGuidOptionString(propertyMessage.GeoCoverageId).Map(guid => new GeoCoverageId(guid)),
            Optional(propertyMessage.CreatedOn).Map(x => x.ToDateTimeOffset()),
            Optional(propertyMessage.LastModifiedOn).Map(x => x.ToDateTimeOffset()),
            NonEmptyText.NewOptionUnvalidated(propertyMessage.PropertyFullName),
            NonEmptyText.NewOptionUnvalidated(propertyMessage.PropertyBaseName),
            NonEmptyText.NewOptionUnvalidated(propertyMessage.StoreNumber));
}