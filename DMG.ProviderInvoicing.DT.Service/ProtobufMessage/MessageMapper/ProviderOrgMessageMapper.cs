using LanguageExt;
using static LanguageExt.Prelude;
using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Domain;
using static DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper.MessageMapperUtility;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;

public static class ProviderOrgMessageMapper
{
    // determine if provider address message is a payment address
    private static bool IsPaymentAddress(Dmg.Providers.V1.ProviderAddressRecord message) =>
        Optional(message.Value)
            .Map(providerAddressMessage => providerAddressMessage.AddressCase is Dmg.Providers.V1.ProviderAddress.AddressOneofCase.PaymentAddress)
            .IfNone(false);

    private static DT.Domain.ProviderOrgAddress ToEntity(Dmg.Types.Address message, string messageAddressId) =>
        new DT.Domain.ProviderOrgAddress(
            new ProviderOrgAddressId(ParseGuidStringDefaultToEmptyGuid(messageAddressId)),
            NonEmptyText.NewOptionUnvalidated(message.Line1),
            NonEmptyText.NewOptionUnvalidated(message.Line2),
            NonEmptyText.NewOptionUnvalidated(message.City),
            NonEmptyText.NewOptionUnvalidated(message.County),
            NonEmptyText.NewOptionUnvalidated(message.State),
            NonEmptyText.NewOptionUnvalidated(message.Neighborhood),
            NonEmptyText.NewOptionUnvalidated(message.PostalCode));
    
    /// If a payment address exists in the address messages, then map to entity
    private static Option<DT.Domain.ProviderOrgAddress> TryToEntityPaymentProviderOrgAddress(IEnumerable<Dmg.Providers.V1.ProviderAddressRecord> messages) =>
        messages.Freeze()
            .Filter(IsPaymentAddress)
            .ToOption()
            .Bind(providerAddressRecord => 
                Optional(providerAddressRecord.Value) // avoid a null ref
                    .Map(providerAddress => ToEntity(providerAddress.PaymentAddress, providerAddressRecord.Id)));    
   
    public static DT.Domain.ProviderOrg ToEntity(Dmg.Providers.V1.ProviderOrg message, ProviderOrgId providerOrgId) => 
        new DT.Domain.ProviderOrg(
            providerOrgId, // providerOrgId is not on the message?!
            NonEmptyText.NewUnsafe(message.Name.DefaultIfNullOrWhiteSpace(DefaultRequiredStringValueIfMissing)),
            NonEmptyText.NewOptionUnvalidated(message.Email),
            TryToEntityPaymentProviderOrgAddress(message.Addresses));
}