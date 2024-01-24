using DMG.ProviderInvoicing.DT.Domain;
using DMG.ProviderInvoicing.BL.Utility;
using LanguageExt;
using static LanguageExt.Prelude;
using static DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper.MessageMapperUtility;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;

/// Provides deterministic mapping between customer protobuf messages and domain entities. It provides shared functionality
/// among any I/O adapter working with protobuf messages.
public static class CustomerMessageMapper 
{
    private static DT.Domain.Customer BuildCustomerEmpty() =>
        new Customer(new CustomerId(Guid.Empty),DefaultRequiredStringValueIfMissing,Option<NonEmptyText>.None,Option<LogoPhotoId>.None);

    public static DT.Domain.Customer ToEntity(DMG.Proto.Customers.Customer customerMessage) =>
        Optional(customerMessage)
            .Match(cm => 
                    new Customer(new CustomerId(ParseGuidStringDefaultToEmptyGuid(cm.CustomerId)),
                        cm.Name.DefaultIfNullOrWhiteSpace(DefaultRequiredStringValueIfMissing),
                        NonEmptyText.NewOptionUnvalidated(cm.CustomerNumber),
                        ParseGuidOptionString(cm.LogoPhotoId).Map(guid => new LogoPhotoId(guid))),
                BuildCustomerEmpty);
}