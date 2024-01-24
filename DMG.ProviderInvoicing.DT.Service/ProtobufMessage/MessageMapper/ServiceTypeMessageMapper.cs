using DMG.ProviderInvoicing.BL.Utility;
using LanguageExt;
using static LanguageExt.Prelude;
using static DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper.MessageMapperUtility;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;

/// Provides deterministic mapping between service type protobuf messages and domain entities. It provides shared functionality
/// among any I/O adapter working with protobuf messages.
public static class ServiceTypeMessageMapper 
{
    public static DT.Domain.ServiceType ToEntity(DMG.DataServices.ServiceType serviceTypeMessage) =>
        new(new(ParseGuidStringDefaultToEmptyGuid(serviceTypeMessage.ServiceTypeId)),
            // TODO - (MW-20220725) - These (Name and Code) fields should be NonEmptyText.
            serviceTypeMessage.Name,
            serviceTypeMessage.Code,
            // optionals
            NonEmptyText.NewOptionUnvalidated(serviceTypeMessage.Description));
}