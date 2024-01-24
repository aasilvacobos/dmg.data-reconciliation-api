using LanguageExt;
using static LanguageExt.Prelude;
using DMG.ProviderInvoicing.BL.Utility;
using static DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper.MessageMapperUtility;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;

/// Provides deterministic mapping between service line protobuf messages and domain entities. It provides shared functionality
/// among any I/O adapter working with protobuf messages.
public static class ServiceLineMessageMapper 
{
    public static DT.Domain.ServiceLine ToEntity(DMG.DataServices.ServiceLine serviceLineMessage) =>
        new(new(ParseGuidStringDefaultToEmptyGuid(serviceLineMessage.ServiceLineId)),
// TODO - (MW-20220725) - This field should be NonEmptyText.
            serviceLineMessage.Name,
            NonEmptyText.NewOptionUnvalidated(serviceLineMessage.Description));
}