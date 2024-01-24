using DMG.Common;
using DMG.ProviderInvoicing.DT.Domain;
using LanguageExt;
using static LanguageExt.Prelude;
using static DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper.MessageMapperUtility;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;

/// Provides deterministic mapping between record meta protobuf messages and domain entities. It provides shared functionality
/// among any I/O adapter working with protobuf messages.
public static class RecordMetaMessageMapper
{
    public static DT.Domain.RecordMeta BuildRecordMetaEmpty() =>
        new (Option<UserId>.None,
            Option<DateTimeOffset>.None,
            Option<UserId>.None, 
            Option<DateTimeOffset>.None);
    
    /// Map job photo message to job photo entity 
    public static DT.Domain.RecordMeta ToEntity(Option<RecordMetaData> recordMetaDataMessageOption) =>
        recordMetaDataMessageOption
            .Match(rmd => new DT.Domain.RecordMeta(ParseGuidOptionString(rmd.CreatedByUserId).Map(guid => new UserId(guid)),
                                                            TryToDateTimeOffset(rmd.CreatedUtc),
                                                            ParseGuidOptionString(rmd.ModifiedByUserId).Map(guid => new UserId(guid)),
                                                            TryToDateTimeOffset(rmd.ModifiedUtc)),
                BuildRecordMetaEmpty);
}