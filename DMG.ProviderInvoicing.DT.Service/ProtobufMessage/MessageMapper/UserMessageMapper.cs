using LanguageExt;
using static LanguageExt.Prelude;
using DMG.ProviderInvoicing.DT.Domain;
using static DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper.MessageMapperUtility;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;

/// Provides deterministic mapping between user protobuf messages and domain entities. It provides shared functionality
/// among any I/O adapter working with protobuf messages.
public static class UserMessageMapper 
{
    public static DT.Domain.User ToEntity(UserId userId, Dmg.Usersor.UserSource userSource) =>
        new(userId,
            userSource.FirstName,
            userSource.LastName);
    
    public static Option<DT.Domain.User> TryToEntity(Dmg.Usersor.User userMessage) =>
        Optional(userMessage.UserSource)
            .Map(x => x.Filter(y => y.IsDefault))   // if there is no default, treat the user as not found
            .Bind (x => x.ToOption())
            .Map(us => ToEntity(new(ParseGuidStringDefaultToEmptyGuid(userMessage.UserId)), us));
}